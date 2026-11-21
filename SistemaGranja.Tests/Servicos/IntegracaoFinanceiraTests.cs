using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    public class IntegracaoFinanceiraTests
    {
        [Fact]
        public async Task Venda_de_lote_divide_o_valor_tira_do_rebanho_e_lanca_receita_no_lote()
        {
            using var db = BancoDeTeste.Criar();
            var galpao = BancoDeTeste.NovoGalpao(db, capacidade: 10);
            var lote = new Lote { CodigoLote = "ENG-01" };
            db.Lotes.Add(lote);
            db.SaveChanges();
            var ids = new[] { "001", "002", "003" }
                .Select(t => BancoDeTeste.NovoAnimal(db, t, galpao.Id, lote.Id).Id)
                .ToList();

            var resultado = await new SaidaDeAnimaisService(db).RegistrarSaidaAsync(new DadosSaida
            {
                AnimalIds = ids,
                Motivo = MotivoSaidaAnimal.Venda,
                ValorTotal = 100m,
                Comprador = "Frigorífico Boi Bom",
                LoteId = lote.Id
            });

            Assert.Null(resultado.Erro);
            Assert.Equal(3, resultado.Quantidade);
            var receita = Assert.Single(db.LancamentosFinanceiros);
            Assert.Equal(TipoLancamento.Receita, receita.Tipo);
            Assert.Equal(100m, receita.Valor);
            Assert.Equal(lote.Id, receita.LoteId);
            Assert.True(receita.Pago);
            Assert.Contains("Frigorífico Boi Bom", receita.Descricao);
            Assert.Equal(CategoriasDoSistema.VendaDeAnimais, db.CategoriasFinanceiras.Find(receita.CategoriaId)!.Nome);

            var vendidos = db.Animais.OrderBy(a => a.TagIdentificacao).ToList();
            Assert.All(vendidos, a => Assert.False(a.Ativo));
            Assert.All(vendidos, a => Assert.Null(a.GalpaoId));
            Assert.All(vendidos, a => Assert.Equal(lote.Id, a.LoteId)); // continua no histórico do lote
            Assert.Equal(new[] { 33.33m, 33.33m, 33.34m }, vendidos.Select(a => a.ValorVenda!.Value));

            // Somem das listas e liberam o galpão
            Assert.Empty(await new AnimalService(db).ObterTodosComRelacionamentosAsync());
            Assert.Equal(3, (await new AnimalService(db).ObterFiltradosAsync(null, null, null, null, foraDoRebanho: true)).Count());
            Assert.Equal(0, await new GalpaoService(db).ContarAnimaisAsync(galpao.Id));
            Assert.Empty(await new LoteService(db).ObterQuantidadeAnimaisAsync());
        }

        [Fact]
        public async Task Morte_tira_do_rebanho_sem_lancar_receita_e_pode_ser_desfeita()
        {
            using var db = BancoDeTeste.Criar();
            var animal = BancoDeTeste.NovoAnimal(db, "010");
            var servico = new SaidaDeAnimaisService(db);

            var resultado = await servico.RegistrarSaidaAsync(new DadosSaida
            {
                AnimalIds = { animal.Id },
                Motivo = MotivoSaidaAnimal.Morte,
                ValorTotal = 500m // ignorado: só venda gera receita
            });

            Assert.Null(resultado.Erro);
            Assert.Null(resultado.Receita);
            Assert.Empty(db.LancamentosFinanceiros);
            Assert.False(db.Animais.Find(animal.Id)!.Ativo);
            Assert.Null(db.Animais.Find(animal.Id)!.ValorVenda);

            Assert.Null(await servico.VoltarAoRebanhoAsync(animal.Id));
            Assert.True(db.Animais.Find(animal.Id)!.Ativo);
            Assert.NotNull(await servico.VoltarAoRebanhoAsync(animal.Id)); // já está no rebanho
        }

        [Fact]
        public async Task Brinco_de_animal_vendido_pode_ser_reaproveitado()
        {
            using var db = BancoDeTeste.Criar();
            var vendido = BancoDeTeste.NovoAnimal(db, "BR-7");
            await new SaidaDeAnimaisService(db).RegistrarSaidaAsync(new DadosSaida { AnimalIds = { vendido.Id }, Motivo = MotivoSaidaAnimal.Venda });

            Assert.False(await new AnimalService(db).ExisteTagAsync("BR-7"));

            // Mas o vendido não volta ao rebanho se o brinco já foi reutilizado
            BancoDeTeste.NovoAnimal(db, "BR-7");
            Assert.NotNull(await new SaidaDeAnimaisService(db).VoltarAoRebanhoAsync(vendido.Id));
        }

        [Fact]
        public async Task Saida_valida_animais_e_data()
        {
            using var db = BancoDeTeste.Criar();
            var animal = BancoDeTeste.NovoAnimal(db, "020");
            var servico = new SaidaDeAnimaisService(db);

            Assert.NotNull((await servico.RegistrarSaidaAsync(new DadosSaida())).Erro);
            Assert.NotNull((await servico.RegistrarSaidaAsync(new DadosSaida { AnimalIds = { animal.Id }, Data = DateTime.Today.AddDays(2) })).Erro);
            Assert.True(db.Animais.Find(animal.Id)!.Ativo);
        }

        [Fact]
        public async Task Editar_animal_nao_desfaz_a_saida()
        {
            using var db = BancoDeTeste.Criar();
            var animal = BancoDeTeste.NovoAnimal(db, "030");
            await new SaidaDeAnimaisService(db).RegistrarSaidaAsync(new DadosSaida { AnimalIds = { animal.Id }, Motivo = MotivoSaidaAnimal.Abate });
            db.ChangeTracker.Clear();

            // A tela de edição não envia os campos de saída
            var editado = new Animal { Id = animal.Id, TagIdentificacao = "030", Nome = "Novo nome", Sexo = "Fêmea", StatusSaude = "Saudável", DataNascimento = animal.DataNascimento };
            await new AnimalService(db).AtualizarAsync(editado);
            db.ChangeTracker.Clear();

            var salvo = db.Animais.Find(animal.Id)!;
            Assert.Equal("Novo nome", salvo.Nome);
            Assert.False(salvo.Ativo);
            Assert.Equal(MotivoSaidaAnimal.Abate, salvo.MotivoSaida);
        }

        private static MovimentacaoEstoqueAlimento Compra(TipoAlimento alimento, double quantidade, decimal preco) => new()
        {
            TipoAlimentoId = alimento.Id,
            TipoMovimentacao = TipoMovimentacaoEstoque.EntradaCompra,
            Quantidade = quantidade,
            PrecoUnitario = preco,
            NotaFiscal = "123"
        };

        [Fact]
        public async Task Compra_no_estoque_vira_despesa_e_acompanha_edicao_e_exclusao()
        {
            using var db = BancoDeTeste.Criar();
            var milho = BancoDeTeste.NovoAlimento(db, "Milho");
            var estoque = new EstoqueService(db);

            var compra = Compra(milho, 10, 2.5m);
            await estoque.AdicionarAsync(compra, lancarNoFinanceiro: true);

            var despesa = Assert.Single(db.LancamentosFinanceiros);
            Assert.Equal(TipoLancamento.Despesa, despesa.Tipo);
            Assert.Equal(25m, despesa.Valor);
            Assert.True(despesa.Pago);
            Assert.Equal(compra.Id, despesa.MovimentacaoEstoqueId);
            Assert.Contains("Milho", despesa.Descricao);
            Assert.Equal(CategoriasDoSistema.CompraDeAlimento, db.CategoriasFinanceiras.Find(despesa.CategoriaId)!.Nome);

            // Editar a compra atualiza a despesa
            db.ChangeTracker.Clear();
            var editada = Compra(milho, 20, 2.5m);
            editada.Id = compra.Id;
            await estoque.AtualizarAsync(editada);
            Assert.Equal(50m, db.LancamentosFinanceiros.Single().Valor);

            // Excluir a compra tira a despesa do financeiro
            db.ChangeTracker.Clear();
            await estoque.RemoverAsync(compra.Id);
            Assert.Empty(db.LancamentosFinanceiros);
        }

        [Fact]
        public async Task Compra_sem_preco_ou_sem_marcar_nao_lanca_e_a_prazo_fica_em_aberto()
        {
            using var db = BancoDeTeste.Criar();
            var milho = BancoDeTeste.NovoAlimento(db, "Milho");
            var estoque = new EstoqueService(db);

            await estoque.AdicionarAsync(Compra(milho, 10, 0m), lancarNoFinanceiro: true);
            await estoque.AdicionarAsync(Compra(milho, 10, 3m), lancarNoFinanceiro: false);
            Assert.Empty(db.LancamentosFinanceiros);

            await estoque.AdicionarAsync(Compra(milho, 4, 10m), lancarNoFinanceiro: true, jaPago: false);
            var conta = Assert.Single(db.LancamentosFinanceiros);
            Assert.False(conta.Pago);
            Assert.Null(conta.DataPagamento);
        }
    }
}
