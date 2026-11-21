using SistemaGranja.Data;
using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    public class FinanceiroServiceTests
    {
        private static async Task<(FinanceiroService Servico, CategoriaFinanceira Venda, CategoriaFinanceira Racao)> PrepararAsync(AppDbContext db)
        {
            var servico = new FinanceiroService(db);
            await servico.GarantirCategoriasPadraoAsync();
            var categorias = await servico.ObterCategoriasAsync();
            return (servico,
                categorias.Single(c => c.Nome == "Venda de animais"),
                categorias.Single(c => c.Nome == "Ração e alimentação"));
        }

        private static LancamentoFinanceiro Novo(TipoLancamento tipo, CategoriaFinanceira categoria, decimal valor, DateTime data, string descricao = "Teste") =>
            new() { Tipo = tipo, CategoriaId = categoria.Id, Valor = valor, Data = data, Descricao = descricao };

        [Fact]
        public async Task Resumo_soma_receitas_despesas_e_saldo_do_periodo()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, venda, racao) = await PrepararAsync(db);
            var hoje = DateTime.Today;

            Assert.Null(await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Receita, venda, 5000m, hoje, "Venda de bezerros")));
            Assert.Null(await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 1200.555m, hoje, "Ração")));
            Assert.Null(await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 300m, hoje.AddYears(-1), "Fora do período")));

            var resumo = await servico.ObterResumoAsync(hoje.AddDays(-1), hoje);

            Assert.Equal(5000m, resumo.Receitas);
            Assert.Equal(1200.56m, resumo.Despesas); // arredonda para centavos
            Assert.Equal(3799.44m, resumo.Saldo);
            Assert.Equal("Venda de animais", resumo.PorCategoria.First().Categoria);
        }

        [Fact]
        public async Task Nao_aceita_categoria_de_tipo_diferente_nem_valor_zero()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, venda, racao) = await PrepararAsync(db);

            Assert.NotNull(await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, venda, 10m, DateTime.Today)));
            Assert.NotNull(await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 0m, DateTime.Today)));
            Assert.Empty(db.LancamentosFinanceiros);
        }

        [Fact]
        public async Task Filtra_por_periodo_tipo_e_busca()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, venda, racao) = await PrepararAsync(db);
            var hoje = DateTime.Today;
            await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Receita, venda, 100m, hoje, "Venda de ovelhas"));
            await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 50m, hoje, "Milho moído"));
            await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 70m, hoje.AddMonths(-2), "Milho antigo"));

            var despesasDoDia = await servico.ObterLancamentosAsync(new FiltroLancamentos { De = hoje, Ate = hoje, Tipo = TipoLancamento.Despesa });
            Assert.Equal("Milho moído", Assert.Single(despesasDoDia).Descricao);

            var busca = await servico.ObterLancamentosAsync(new FiltroLancamentos { Busca = "MILHO" });
            Assert.Equal(2, busca.Count);
        }

        [Fact]
        public async Task Editar_lancamento_altera_os_dados()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, _, racao) = await PrepararAsync(db);
            var lancamento = Novo(TipoLancamento.Despesa, racao, 80m, DateTime.Today, "Sal");
            await servico.SalvarLancamentoAsync(lancamento);

            var alterado = Novo(TipoLancamento.Despesa, racao, 95.5m, DateTime.Today, "Sal mineral");
            alterado.Id = lancamento.Id;
            Assert.Null(await servico.SalvarLancamentoAsync(alterado));

            var salvo = await servico.ObterLancamentoAsync(lancamento.Id);
            Assert.Equal(95.5m, salvo!.Valor);
            Assert.Equal("Sal mineral", salvo.Descricao);
        }

        [Fact]
        public async Task Categoria_com_lancamentos_nao_e_excluida_mas_sem_lancamentos_e()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, venda, racao) = await PrepararAsync(db);
            await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 10m, DateTime.Today));

            Assert.NotNull(await servico.RemoverCategoriaAsync(racao.Id));
            Assert.Null(await servico.RemoverCategoriaAsync(venda.Id));
            Assert.Null(await servico.ObterCategoriaAsync(venda.Id));
        }

        [Fact]
        public async Task Categorias_padrao_criadas_uma_vez_e_sem_nome_repetido()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, _, _) = await PrepararAsync(db);
            var total = db.CategoriasFinanceiras.Count();

            await servico.GarantirCategoriasPadraoAsync();
            Assert.Equal(total, db.CategoriasFinanceiras.Count());
            Assert.Contains(db.CategoriasFinanceiras, c => c.Tipo == TipoLancamento.Receita);

            var repetida = new CategoriaFinanceira { Nome = "ração e alimentação", Tipo = TipoLancamento.Despesa };
            Assert.NotNull(await servico.SalvarCategoriaAsync(repetida));

            // Mesmo nome em outro tipo pode
            var outroTipo = new CategoriaFinanceira { Nome = "Ração e alimentação", Tipo = TipoLancamento.Receita };
            Assert.Null(await servico.SalvarCategoriaAsync(outroTipo));
        }

        [Fact]
        public async Task Cada_fazenda_ve_so_o_proprio_financeiro()
        {
            var banco = BancoDeTeste.NovoNomeDeBanco();
            using (var b = BancoDeTeste.CriarParaFazenda(banco, "Fazenda B"))
            {
                var (servicoB, vendaB, _) = await PrepararAsync(b);
                await servicoB.SalvarLancamentoAsync(Novo(TipoLancamento.Receita, vendaB, 999m, DateTime.Today));
            }

            using var a = BancoDeTeste.CriarParaFazenda(banco, "Fazenda A");
            var (servicoA, _, _) = await PrepararAsync(a);

            Assert.Empty(await servicoA.ObterLancamentosAsync(new FiltroLancamentos()));
            Assert.Equal(0m, (await servicoA.ObterResumoAsync(DateTime.Today, DateTime.Today)).Receitas);
        }

        [Fact]
        public async Task Conta_em_aberto_fica_fora_do_realizado_ate_ser_paga()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, _, racao) = await PrepararAsync(db);
            var hoje = DateTime.Today;
            var conta = Novo(TipoLancamento.Despesa, racao, 400m, hoje, "Boleto da ração");
            conta.Pago = false;
            conta.DataPagamento = hoje; // ignorado: conta em aberto não tem data de pagamento
            Assert.Null(await servico.SalvarLancamentoAsync(conta));
            Assert.Null(conta.DataPagamento);

            var antes = await servico.ObterResumoAsync(hoje, hoje);
            Assert.Equal(0m, antes.Despesas);
            Assert.Equal(400m, antes.APagar);

            Assert.Null(await servico.MarcarComoPagoAsync(conta.Id, hoje));
            Assert.NotNull(await servico.MarcarComoPagoAsync(conta.Id, hoje)); // já estava paga

            var depois = await servico.ObterResumoAsync(hoje, hoje);
            Assert.Equal(400m, depois.Despesas);
            Assert.Equal(0m, depois.APagar);
        }

        [Fact]
        public async Task Repetir_cria_uma_conta_por_mes_e_as_seguintes_ficam_em_aberto()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, _, racao) = await PrepararAsync(db);
            var data = new DateTime(2026, 1, 31);

            Assert.Null(await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 250m, data, "Financiamento"), 3));

            var parcelas = db.LancamentosFinanceiros.OrderBy(l => l.Data).ToList();
            Assert.Equal(3, parcelas.Count);
            Assert.Equal("Financiamento (1/3)", parcelas[0].Descricao);
            Assert.Equal("Financiamento (3/3)", parcelas[2].Descricao);
            Assert.Equal(new DateTime(2026, 2, 28), parcelas[1].Data); // fevereiro não tem dia 31
            Assert.True(parcelas[0].Pago);
            Assert.False(parcelas[1].Pago);
            Assert.False(parcelas[2].Pago);

            Assert.NotNull(await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 1m, data), 99));
        }

        [Fact]
        public async Task Contas_em_aberto_separadas_por_urgencia()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, venda, racao) = await PrepararAsync(db);
            var hoje = DateTime.Today;
            foreach (var (dias, categoria, tipo) in new[] { (-5, racao, TipoLancamento.Despesa), (3, racao, TipoLancamento.Despesa), (20, venda, TipoLancamento.Receita) })
            {
                var conta = Novo(tipo, categoria, 100m, hoje.AddDays(dias));
                conta.Pago = false;
                await servico.SalvarLancamentoAsync(conta);
            }
            await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 999m, hoje)); // paga: não aparece

            var contas = await servico.ObterContasEmAbertoAsync(null, hoje);

            Assert.Single(contas.Vencidas);
            Assert.Single(contas.ProximosSeteDias);
            Assert.Single(contas.Depois);
            Assert.Equal(200m, contas.TotalAPagar);
            Assert.Equal(100m, contas.TotalAReceber);
            Assert.Single((await servico.ObterContasEmAbertoAsync(TipoLancamento.Receita, hoje)).Todas);
        }

        [Fact]
        public async Task Fluxo_de_caixa_acumula_saldo_e_preve_contas_em_aberto()
        {
            using var db = BancoDeTeste.Criar();
            var (servico, venda, racao) = await PrepararAsync(db);
            var hoje = new DateTime(2026, 6, 15);

            await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Receita, venda, 1000m, new DateTime(2026, 1, 10))); // antes do período
            await servico.SalvarLancamentoAsync(Novo(TipoLancamento.Despesa, racao, 300m, new DateTime(2026, 5, 5)));   // mês passado
            var vencida = Novo(TipoLancamento.Despesa, racao, 100m, new DateTime(2026, 5, 20));                         // vencida: entra em junho
            vencida.Pago = false;
            await servico.SalvarLancamentoAsync(vencida);
            var futura = Novo(TipoLancamento.Receita, venda, 500m, new DateTime(2026, 8, 1));                           // a receber em agosto
            futura.Pago = false;
            await servico.SalvarLancamentoAsync(futura);

            var fluxo = await servico.ObterFluxoDeCaixaAsync(hoje, mesesAntes: 2, mesesDepois: 3);

            Assert.Equal(6, fluxo.Meses.Count);                    // abril a setembro
            Assert.Equal(1000m, fluxo.SaldoInicial);
            Assert.Equal(700m, fluxo.SaldoAtual);
            var maio = fluxo.Meses.Single(m => m.Mes.Month == 5);
            Assert.Equal(300m, maio.SaidasRealizadas);
            Assert.Equal(0m, maio.SaidasPrevistas);
            var junho = fluxo.Meses.Single(m => m.Atual);
            Assert.Equal(100m, junho.SaidasPrevistas);
            Assert.Equal(600m, junho.SaldoAcumulado);
            Assert.Equal(500m, fluxo.Meses.Single(m => m.Mes.Month == 8).EntradasPrevistas);
            Assert.Equal(1100m, fluxo.Meses.Last().SaldoAcumulado);
        }

        [Fact]
        public void Periodos_prontos_do_financeiro()
        {
            var hoje = new DateTime(2026, 3, 15);

            Assert.Equal((new DateTime(2026, 3, 1), new DateTime(2026, 3, 31)), FinanceiroController.CalcularPeriodo("mes", hoje));
            Assert.Equal((new DateTime(2026, 2, 1), new DateTime(2026, 2, 28)), FinanceiroController.CalcularPeriodo("mes-passado", hoje));
            Assert.Equal((new DateTime(2026, 2, 14), hoje), FinanceiroController.CalcularPeriodo("30dias", hoje));
            Assert.Equal((new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)), FinanceiroController.CalcularPeriodo("ano", hoje));
        }
    }
}
