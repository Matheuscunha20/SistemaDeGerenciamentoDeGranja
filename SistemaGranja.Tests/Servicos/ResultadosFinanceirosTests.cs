using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    public class ResultadosFinanceirosTests
    {
        [Fact]
        public async Task Lucro_do_lote_soma_receitas_e_custos_ligados_a_ele()
        {
            using var db = BancoDeTeste.Criar();
            var financeiro = new FinanceiroService(db);
            await financeiro.GarantirCategoriasPadraoAsync();
            var racao = db.CategoriasFinanceiras.Single(c => c.Nome == "Ração e alimentação");
            var lote = new Lote { CodigoLote = "ENG-01" };
            var outro = new Lote { CodigoLote = "ENG-02" };
            db.Lotes.AddRange(lote, outro);
            db.SaveChanges();
            var ids = new[] { "001", "002" }.Select(t => BancoDeTeste.NovoAnimal(db, t, loteId: lote.Id).Id).ToList();
            BancoDeTeste.NovoAnimal(db, "003", loteId: lote.Id);

            // Custos: ração paga e vacina a pagar; um custo sem lote não entra
            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Despesa, CategoriaId = racao.Id, Valor = 600m, Descricao = "Ração", LoteId = lote.Id });
            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Despesa, CategoriaId = racao.Id, Valor = 300m, Descricao = "Ração a prazo", LoteId = lote.Id, Pago = false });
            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Despesa, CategoriaId = racao.Id, Valor = 999m, Descricao = "Geral" });

            // Receita: venda de 2 animais do lote
            await new SaidaDeAnimaisService(db).RegistrarSaidaAsync(new DadosSaida { AnimalIds = ids, Motivo = MotivoSaidaAnimal.Venda, ValorTotal = 1200m, LoteId = lote.Id });

            var resultado = await financeiro.ObterResultadoDoLoteAsync(lote.Id);
            Assert.NotNull(resultado);
            Assert.Equal(1200m, resultado!.Receitas);
            Assert.Equal(900m, resultado.Despesas);
            Assert.Equal(300m, resultado.Lucro);
            Assert.Equal(0.25m, resultado.Margem);
            Assert.Equal(300m, resultado.EmAberto);
            Assert.Equal(1, resultado.AnimaisNoLote);
            Assert.Equal(2, resultado.AnimaisQueSairam);
            Assert.Equal(300m, resultado.CustoPorAnimal);
            Assert.Equal(3, resultado.Lancamentos.Count);

            // Na lista por lote, só aparece lote com lançamentos
            var lista = await financeiro.ObterResultadosPorLoteAsync();
            var item = Assert.Single(lista);
            Assert.Equal("ENG-01", item.Lote.CodigoLote);
            Assert.Equal(300m, item.Lucro);
            Assert.Equal(2, (await financeiro.ObterResultadosPorLoteAsync(somenteComLancamentos: false)).Count);
        }

        [Fact]
        public async Task Totais_mensais_contam_so_o_que_foi_pago_em_cada_mes()
        {
            using var db = BancoDeTeste.Criar();
            var financeiro = new FinanceiroService(db);
            await financeiro.GarantirCategoriasPadraoAsync();
            var venda = db.CategoriasFinanceiras.Single(c => c.Nome == "Venda de animais");
            var racao = db.CategoriasFinanceiras.Single(c => c.Nome == "Ração e alimentação");
            var hoje = new DateTime(2026, 6, 10);

            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Receita, CategoriaId = venda.Id, Valor = 1000m, Descricao = "Venda", Data = new DateTime(2026, 6, 2) });
            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Despesa, CategoriaId = racao.Id, Valor = 400m, Descricao = "Ração", Data = new DateTime(2026, 4, 20) });
            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Despesa, CategoriaId = racao.Id, Valor = 50m, Descricao = "Em aberto", Data = new DateTime(2026, 6, 5), Pago = false });
            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Despesa, CategoriaId = racao.Id, Valor = 70m, Descricao = "Antigo", Data = new DateTime(2025, 1, 5) });

            var meses = await financeiro.ObterTotaisMensaisAsync(hoje, 6);

            Assert.Equal(6, meses.Count);
            Assert.Equal(new DateTime(2026, 1, 1), meses.First().Mes);
            Assert.Equal(new DateTime(2026, 6, 1), meses.Last().Mes);
            Assert.Equal(1000m, meses.Last().Receitas);
            Assert.Equal(0m, meses.Last().Despesas);
            Assert.Equal(400m, meses.Single(m => m.Mes.Month == 4).Despesas);
        }

        [Fact]
        public async Task Painel_mostra_o_financeiro_do_mes()
        {
            using var db = BancoDeTeste.Criar();
            var financeiro = new FinanceiroService(db);
            await financeiro.GarantirCategoriasPadraoAsync();
            var venda = db.CategoriasFinanceiras.Single(c => c.Nome == "Venda de animais");
            var racao = db.CategoriasFinanceiras.Single(c => c.Nome == "Ração e alimentação");
            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Receita, CategoriaId = venda.Id, Valor = 800m, Descricao = "Venda" });
            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Despesa, CategoriaId = racao.Id, Valor = 200m, Descricao = "Ração" });
            await financeiro.SalvarLancamentoAsync(new LancamentoFinanceiro { Tipo = TipoLancamento.Receita, CategoriaId = venda.Id, Valor = 150m, Descricao = "A receber", Data = DateTime.Today.AddDays(3), Pago = false });

            var estoque = new EstoqueService(db);
            var resumo = await new PainelService(db, new GalpaoService(db), new VacinacaoService(db), estoque).ObterResumoAsync();

            Assert.Equal(800m, resumo.RecebidoNoMes);
            Assert.Equal(200m, resumo.PagoNoMes);
            Assert.Equal(600m, resumo.SaldoDoMes);
            Assert.Equal(150m, resumo.AReceberProximos7Dias);
        }
    }
}
