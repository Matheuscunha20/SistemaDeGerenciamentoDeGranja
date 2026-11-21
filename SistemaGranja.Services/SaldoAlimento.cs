using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    public enum SituacaoEstoque
    {
        Normal,
        Baixo,   // saldo igual ou abaixo do estoque mínimo
        Zerado
    }

    // Resumo do estoque de um alimento, calculado a partir das movimentações
    public class SaldoAlimento
    {
        public TipoAlimento Alimento { get; set; } = null!;

        public double Saldo { get; set; }

        // Média de consumo por dia nos últimos 30 dias
        public double ConsumoMedioDiario { get; set; }

        // Quantos dias o saldo dura no ritmo atual de consumo (null se não houve consumo)
        public double? DiasRestantes => ConsumoMedioDiario > 0 ? Saldo / ConsumoMedioDiario : null;

        // Preço unitário da compra mais recente
        public decimal? UltimoPreco { get; set; }

        public SituacaoEstoque Situacao =>
            Saldo <= 0 ? SituacaoEstoque.Zerado
            : Alimento.EstoqueMinimo.HasValue && Saldo <= Alimento.EstoqueMinimo.Value ? SituacaoEstoque.Baixo
            : SituacaoEstoque.Normal;
    }
}
