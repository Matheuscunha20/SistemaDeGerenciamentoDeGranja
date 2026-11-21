using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Tests.Dominio
{
    public class EstoqueDominioTests
    {
        [Theory]
        [InlineData(TipoMovimentacaoEstoque.EntradaCompra, true)]
        [InlineData(TipoMovimentacaoEstoque.AjusteInventarioPositivo, true)]
        [InlineData(TipoMovimentacaoEstoque.SaidaConsumo, false)]
        [InlineData(TipoMovimentacaoEstoque.SaidaPerda, false)]
        [InlineData(TipoMovimentacaoEstoque.AjusteInventarioNegativo, false)]
        public void Sabe_se_a_movimentacao_e_entrada_ou_saida(TipoMovimentacaoEstoque tipo, bool esperado)
        {
            Assert.Equal(esperado, tipo.EhEntrada());
        }

        [Fact]
        public void Saida_tem_quantidade_negativa_no_saldo()
        {
            var saida = new MovimentacaoEstoqueAlimento { TipoMovimentacao = TipoMovimentacaoEstoque.SaidaConsumo, Quantidade = 30 };
            Assert.Equal(-30, saida.QuantidadeComSinal);
        }

        [Fact]
        public void Valor_total_e_quantidade_vezes_preco()
        {
            var compra = new MovimentacaoEstoqueAlimento
            {
                TipoMovimentacao = TipoMovimentacaoEstoque.EntradaCompra,
                Quantidade = 10,
                PrecoUnitario = 2.50m
            };
            Assert.Equal(25.00m, compra.ValorTotal);
        }

        [Theory]
        [InlineData(0, 100, SituacaoEstoque.Zerado)]
        [InlineData(80, 100, SituacaoEstoque.Baixo)]
        [InlineData(100, 100, SituacaoEstoque.Baixo)]
        [InlineData(101, 100, SituacaoEstoque.Normal)]
        public void Situacao_depende_do_estoque_minimo(double saldo, double minimo, SituacaoEstoque esperado)
        {
            var resumo = new SaldoAlimento { Alimento = new TipoAlimento { EstoqueMinimo = minimo }, Saldo = saldo };
            Assert.Equal(esperado, resumo.Situacao);
        }

        [Fact]
        public void Sem_estoque_minimo_nunca_fica_baixo()
        {
            var resumo = new SaldoAlimento { Alimento = new TipoAlimento(), Saldo = 1 };
            Assert.Equal(SituacaoEstoque.Normal, resumo.Situacao);
        }

        [Fact]
        public void Calcula_quantos_dias_o_estoque_dura()
        {
            var resumo = new SaldoAlimento { Alimento = new TipoAlimento(), Saldo = 500, ConsumoMedioDiario = 25 };
            Assert.Equal(20, resumo.DiasRestantes);
        }

        [Fact]
        public void Sem_consumo_nao_ha_previsao()
        {
            var resumo = new SaldoAlimento { Alimento = new TipoAlimento(), Saldo = 500, ConsumoMedioDiario = 0 };
            Assert.Null(resumo.DiasRestantes);
        }
    }
}
