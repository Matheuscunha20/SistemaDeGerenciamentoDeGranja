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
        public bool? Pago { get; set; } // null = todos; true = pagos/recebidos; false = em aberto
    }

    // Totais de um período
    public class ResumoFinanceiro
    {
        // O que de fato entrou e saiu (pago/recebido dentro do período)
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Saldo => Receitas - Despesas;

        // O que ainda está em aberto com vencimento dentro do período
        public decimal AReceber { get; set; }
        public decimal APagar { get; set; }

        // Total por categoria (só o que foi pago/recebido), do maior para o menor
        public List<(string Categoria, TipoLancamento Tipo, decimal Total)> PorCategoria { get; set; } = new();
    }

    // Contas em aberto, separadas pela urgência
    public class ContasEmAberto
    {
        public List<LancamentoFinanceiro> Vencidas { get; set; } = new();
        public List<LancamentoFinanceiro> ProximosSeteDias { get; set; } = new();
        public List<LancamentoFinanceiro> Depois { get; set; } = new();

        public IEnumerable<LancamentoFinanceiro> Todas => Vencidas.Concat(ProximosSeteDias).Concat(Depois);
        public decimal TotalAPagar => Todas.Where(l => l.Tipo == TipoLancamento.Despesa).Sum(l => l.Valor);
        public decimal TotalAReceber => Todas.Where(l => l.Tipo == TipoLancamento.Receita).Sum(l => l.Valor);
    }

    // Um mês do fluxo de caixa
    public class FluxoMes
    {
        public DateTime Mes { get; set; }
        public bool Passado { get; set; }
        public bool Atual { get; set; }
        public decimal EntradasRealizadas { get; set; }
        public decimal SaidasRealizadas { get; set; }
        public decimal EntradasPrevistas { get; set; }
        public decimal SaidasPrevistas { get; set; }
        public decimal Entradas => EntradasRealizadas + EntradasPrevistas;
        public decimal Saidas => SaidasRealizadas + SaidasPrevistas;
        public decimal SaldoDoMes => Entradas - Saidas;
        public decimal SaldoAcumulado { get; set; }
    }

    public class FluxoDeCaixa
    {
        // Tudo que entrou menos tudo que saiu antes do primeiro mês mostrado
        public decimal SaldoInicial { get; set; }
        // Saldo de hoje (tudo pago/recebido até agora)
        public decimal SaldoAtual { get; set; }
        public List<FluxoMes> Meses { get; set; } = new();
    }

    // Custo, receita e lucro de um lote (lançamentos ligados a ele, pagos ou em aberto)
    public class ResultadoLote
    {
        public Lote Lote { get; set; } = null!;
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Lucro => Receitas - Despesas;
        // Lucro sobre o que entrou (ex.: 0,25 = 25%); null se não houve receita
        public decimal? Margem => Receitas > 0 ? Lucro / Receitas : null;
        public decimal EmAberto { get; set; } // parte dos valores que ainda não foi paga/recebida
        public int AnimaisNoLote { get; set; }
        public int AnimaisQueSairam { get; set; }
        public int TotalDeAnimais => AnimaisNoLote + AnimaisQueSairam;
        public decimal? CustoPorAnimal => TotalDeAnimais > 0 ? Math.Round(Despesas / TotalDeAnimais, 2) : null;
        public List<LancamentoFinanceiro> Lancamentos { get; set; } = new();
    }

    // Receitas e despesas pagas/recebidas em um mês
    public class TotaisDoMes
    {
        public DateTime Mes { get; set; }
        public decimal Receitas { get; set; }
        public decimal Despesas { get; set; }
        public decimal Saldo => Receitas - Despesas;
    }

    public interface IFinanceiroService
    {
        // Lançamentos
        Task<List<LancamentoFinanceiro>> ObterLancamentosAsync(FiltroLancamentos filtro);
        Task<LancamentoFinanceiro?> ObterLancamentoAsync(int id);
        Task<ResumoFinanceiro> ObterResumoAsync(DateTime de, DateTime ate);

        // repetirMeses > 1 cria uma conta por mês (ex.: parcelas), a partir da data informada.
        // Retorna null se deu certo, ou a mensagem de erro.
        Task<string?> SalvarLancamentoAsync(LancamentoFinanceiro lancamento, int repetirMeses = 1);
        Task<bool> RemoverLancamentoAsync(int id);

        // Contas a pagar e a receber
        Task<ContasEmAberto> ObterContasEmAbertoAsync(TipoLancamento? tipo, DateTime hoje);
        Task<string?> MarcarComoPagoAsync(int id, DateTime dataPagamento);
        Task<FluxoDeCaixa> ObterFluxoDeCaixaAsync(DateTime hoje, int mesesAntes = 2, int mesesDepois = 3);

        // Resultados
        Task<List<ResultadoLote>> ObterResultadosPorLoteAsync(bool somenteComLancamentos = true);
        Task<ResultadoLote?> ObterResultadoDoLoteAsync(int loteId);
        Task<List<TotaisDoMes>> ObterTotaisMensaisAsync(DateTime hoje, int meses = 6);

        // Categorias
        Task GarantirCategoriasPadraoAsync();
        Task<List<CategoriaFinanceira>> ObterCategoriasAsync(TipoLancamento? tipo = null, bool somenteAtivas = false);
        Task<CategoriaFinanceira?> ObterCategoriaAsync(int id);
        Task<int> ContarLancamentosDaCategoriaAsync(int categoriaId);
        Task<string?> SalvarCategoriaAsync(CategoriaFinanceira categoria); // null = deu certo
        Task<string?> RemoverCategoriaAsync(int id); // null = deu certo
    }
}
