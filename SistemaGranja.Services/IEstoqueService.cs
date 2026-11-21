using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IEstoqueService
    {
        // Saldo de todos os alimentos cadastrados
        Task<IEnumerable<SaldoAlimento>> ObterSaldosAsync();

        // Saldo de um alimento. ignorarMovimentacaoId: calcula como se essa movimentação não existisse (útil ao editar)
        Task<double> ObterSaldoAsync(int tipoAlimentoId, int? ignorarMovimentacaoId = null);

        Task<IEnumerable<MovimentacaoEstoqueAlimento>> ObterMovimentacoesAsync(
            int? tipoAlimentoId = null, TipoMovimentacaoEstoque? tipo = null, DateTime? de = null, DateTime? ate = null);

        // Leitura sem rastreamento do EF
        Task<MovimentacaoEstoqueAlimento?> ObterPorIdAsync(int id);

        // Total gasto em compras no período
        Task<decimal> ObterGastoComComprasAsync(DateTime de, DateTime ate);

        Task AdicionarAsync(MovimentacaoEstoqueAlimento movimentacao);
        Task AtualizarAsync(MovimentacaoEstoqueAlimento movimentacao);
        Task RemoverAsync(int id);
    }
}
