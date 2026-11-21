using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Servicos
{
    public class EstoqueServiceTests
    {
        private static MovimentacaoEstoqueAlimento Movimento(int alimentoId, TipoMovimentacaoEstoque tipo, double quantidade, decimal preco = 0, int diasAtras = 0) =>
            new()
            {
                TipoAlimentoId = alimentoId,
                TipoMovimentacao = tipo,
                Quantidade = quantidade,
                PrecoUnitario = preco,
                DataMovimentacao = DateTime.Today.AddDays(-diasAtras)
            };

        [Fact]
        public async Task Saldo_soma_entradas_e_subtrai_saidas()
        {
            using var db = BancoDeTeste.Criar();
            var milho = BancoDeTeste.NovoAlimento(db);
            var servico = new EstoqueService(db);

            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.EntradaCompra, 100));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.SaidaConsumo, 30));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.SaidaPerda, 5));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.AjusteInventarioPositivo, 10));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.AjusteInventarioNegativo, 5));

            Assert.Equal(70, await servico.ObterSaldoAsync(milho.Id));
            var saldo = Assert.Single(await servico.ObterSaldosAsync());
            Assert.Equal(70, saldo.Saldo);
        }

        [Fact]
        public async Task Saldo_pode_ignorar_uma_movimentacao()
        {
            using var db = BancoDeTeste.Criar();
            var milho = BancoDeTeste.NovoAlimento(db);
            var servico = new EstoqueService(db);

            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.EntradaCompra, 100));
            var saida = Movimento(milho.Id, TipoMovimentacaoEstoque.SaidaConsumo, 40);
            await servico.AdicionarAsync(saida);

            // Usado ao editar: o saldo "sem esta saída" volta a 100
            Assert.Equal(100, await servico.ObterSaldoAsync(milho.Id, ignorarMovimentacaoId: saida.Id));
        }

        [Fact]
        public async Task Saida_nao_guarda_preco_e_usa_a_unidade_do_alimento()
        {
            using var db = BancoDeTeste.Criar();
            var milho = BancoDeTeste.NovoAlimento(db);
            var servico = new EstoqueService(db);

            var saida = Movimento(milho.Id, TipoMovimentacaoEstoque.SaidaConsumo, 10, preco: 99m);
            saida.Quantidade = -10; // mesmo negativa, é gravada positiva
            await servico.AdicionarAsync(saida);

            Assert.Equal(0m, saida.PrecoUnitario);
            Assert.Equal("Kg", saida.UnidadeMedida);
            Assert.Equal(10, saida.Quantidade);
        }

        [Fact]
        public async Task Fica_baixo_quando_chega_no_estoque_minimo()
        {
            using var db = BancoDeTeste.Criar();
            var milho = BancoDeTeste.NovoAlimento(db, estoqueMinimo: 80);
            var servico = new EstoqueService(db);

            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.EntradaCompra, 100));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.SaidaConsumo, 25));

            var saldo = Assert.Single(await servico.ObterSaldosAsync());
            Assert.Equal(SituacaoEstoque.Baixo, saldo.Situacao);
        }

        [Fact]
        public async Task Consumo_medio_usa_os_ultimos_30_dias()
        {
            using var db = BancoDeTeste.Criar();
            var milho = BancoDeTeste.NovoAlimento(db);
            var servico = new EstoqueService(db);

            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.EntradaCompra, 1000, diasAtras: 60));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.SaidaConsumo, 300, diasAtras: 5));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.SaidaConsumo, 500, diasAtras: 45)); // fora do período
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.SaidaPerda, 50, diasAtras: 2));     // perda não é consumo

            var saldo = Assert.Single(await servico.ObterSaldosAsync());
            Assert.Equal(10, saldo.ConsumoMedioDiario);     // 300 kg / 30 dias
            Assert.Equal(15, saldo.DiasRestantes);          // 150 kg / 10 kg por dia
        }

        [Fact]
        public async Task Soma_o_gasto_com_compras_do_periodo()
        {
            using var db = BancoDeTeste.Criar();
            var milho = BancoDeTeste.NovoAlimento(db);
            var servico = new EstoqueService(db);

            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.EntradaCompra, 10, preco: 2.50m, diasAtras: 3));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.EntradaCompra, 4, preco: 10m, diasAtras: 10));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.EntradaCompra, 100, preco: 1m, diasAtras: 90)); // fora

            var gasto = await servico.ObterGastoComComprasAsync(DateTime.Today.AddDays(-30), DateTime.Today);

            Assert.Equal(65m, gasto); // 25 + 40
        }

        [Fact]
        public async Task Ultimo_preco_e_o_da_compra_mais_recente()
        {
            using var db = BancoDeTeste.Criar();
            var milho = BancoDeTeste.NovoAlimento(db);
            var servico = new EstoqueService(db);

            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.EntradaCompra, 10, preco: 2m, diasAtras: 20));
            await servico.AdicionarAsync(Movimento(milho.Id, TipoMovimentacaoEstoque.EntradaCompra, 10, preco: 3m, diasAtras: 1));

            var saldo = Assert.Single(await servico.ObterSaldosAsync());
            Assert.Equal(3m, saldo.UltimoPreco);
        }
    }
}
