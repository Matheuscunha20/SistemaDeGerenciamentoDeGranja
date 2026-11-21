using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    // Filtro da lista de lançamentos
    public class FiltroLancamentos
    {
        public DateTime? De { get; set; }
        public DateTime? Ate { get; set; }
        public TipoLancamento? Tipo { get; set; }
        public int? CategoriaId { get; set; }
        public string? Busca { get; set; }
    }

    // Totais de um período
    public class ResumoFinanceiro
    {
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Saldo => Receitas - Despesas;

        // Total por categoria, do maior para o menor
        public List<(string Categoria, TipoLancamento Tipo, decimal Total)> PorCategoria { get; set; } = new();
    }

    public interface IFinanceiroService
    {
        // Lançamentos
        Task<List<LancamentoFinanceiro>> ObterLancamentosAsync(FiltroLancamentos filtro);
        Task<LancamentoFinanceiro?> ObterLancamentoAsync(int id);
        Task<ResumoFinanceiro> ObterResumoAsync(DateTime de, DateTime ate);
        Task<string?> SalvarLancamentoAsync(LancamentoFinanceiro lancamento); // null = deu certo
        Task<bool> RemoverLancamentoAsync(int id);

        // Categorias
        Task GarantirCategoriasPadraoAsync();
        Task<List<CategoriaFinanceira>> ObterCategoriasAsync(TipoLancamento? tipo = null, bool somenteAtivas = false);
        Task<CategoriaFinanceira?> ObterCategoriaAsync(int id);
        Task<int> ContarLancamentosDaCategoriaAsync(int categoriaId);
        Task<string?> SalvarCategoriaAsync(CategoriaFinanceira categoria); // null = deu certo
        Task<string?> RemoverCategoriaAsync(int id); // null = deu certo
    }
}
