using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using SistemaGranja.Web.Identidade;

// Receitas e despesas da fazenda. Só Dono e Gerente veem o financeiro.
[Authorize(Policy = Politicas.Gestao)]
public class FinanceiroController : Controller
{
    private readonly IFinanceiroService _financeiro;
    private readonly ILoteService _loteService;

    public FinanceiroController(IFinanceiroService financeiro, ILoteService loteService)
    {
        _financeiro = financeiro;
        _loteService = loteService;
    }

    public class TelaFinanceiro
    {
        public string Periodo { get; set; } = "mes";
        public DateTime De { get; set; }
        public DateTime Ate { get; set; }
        public TipoLancamento? Tipo { get; set; }
        public int? CategoriaId { get; set; }
        public string? Busca { get; set; }
        public string? Situacao { get; set; } // "pagos", "abertos" ou vazio
        public ResumoFinanceiro Resumo { get; set; } = new();
        public List<LancamentoFinanceiro> Lancamentos { get; set; } = new();
        public List<CategoriaFinanceira> Categorias { get; set; } = new();
        public bool FiltroExtra => Tipo.HasValue || CategoriaId.HasValue || !string.IsNullOrWhiteSpace(Busca) || !string.IsNullOrEmpty(Situacao);
    }

    // Períodos prontos: este mês, mês passado, últimos 30 dias, este ano; ou datas escolhidas
    public static (DateTime De, DateTime Ate) CalcularPeriodo(string? periodo, DateTime hoje)
    {
        var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);
        return periodo switch
        {
            "mes-passado" => (inicioDoMes.AddMonths(-1), inicioDoMes.AddDays(-1)),
            "30dias" => (hoje.AddDays(-29), hoje),
            "ano" => (new DateTime(hoje.Year, 1, 1), new DateTime(hoje.Year, 12, 31)),
            _ => (inicioDoMes, inicioDoMes.AddMonths(1).AddDays(-1)),
        };
    }

    // GET: Financeiro
    public async Task<IActionResult> Index(string? periodo, DateTime? de, DateTime? ate, TipoLancamento? tipo, int? categoriaId, string? busca, string? situacao)
    {
        await _financeiro.GarantirCategoriasPadraoAsync();

        DateTime inicio, fim;
        if (de.HasValue || ate.HasValue)
        {
            periodo = "personalizado";
            inicio = (de ?? ate!.Value).Date;
            fim = (ate ?? de!.Value).Date;
            if (fim < inicio) (inicio, fim) = (fim, inicio);
        }
        else
        {
            periodo ??= "mes";
            (inicio, fim) = CalcularPeriodo(periodo, DateTime.Today);
        }

        var tela = new TelaFinanceiro
        {
            Periodo = periodo,
            De = inicio,
            Ate = fim,
            Tipo = tipo,
            CategoriaId = categoriaId,
            Busca = busca,
            Situacao = situacao is "pagos" or "abertos" ? situacao : null,
            Resumo = await _financeiro.ObterResumoAsync(inicio, fim),
            Lancamentos = await _financeiro.ObterLancamentosAsync(new FiltroLancamentos
            {
                De = inicio, Ate = fim, Tipo = tipo, CategoriaId = categoriaId, Busca = busca,
                Pago = situacao == "pagos" ? true : situacao == "abertos" ? false : null
            }),
            Categorias = await _financeiro.ObterCategoriasAsync()
        };
        return View(tela);
    }

    // GET: Financeiro/Create?tipo=Receita
    public async Task<IActionResult> Create(TipoLancamento? tipo)
    {
        await _financeiro.GarantirCategoriasPadraoAsync();
        var lancamento = new LancamentoFinanceiro { Tipo = tipo ?? TipoLancamento.Despesa };
        await CarregarListasAsync(lancamento);
        return View(lancamento);
    }

    // POST: Financeiro/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Tipo,Descricao,Valor,Data,Pago,DataPagamento,CategoriaId,FormaPagamento,LoteId,Observacoes")] LancamentoFinanceiro lancamento, string? salvarENovo, int repetirMeses = 1)
    {
        LimparValidacaoDeNavegacao();
        ViewData["RepetirMeses"] = repetirMeses;
        if (ModelState.IsValid)
        {
            var erro = await _financeiro.SalvarLancamentoAsync(lancamento, repetirMeses);
            if (erro == null)
            {
                var situacao = lancamento.Pago ? "registrada" : "lançada como conta em aberto";
                TempData["Sucesso"] = repetirMeses > 1
                    ? $"{repetirMeses} {(lancamento.Tipo == TipoLancamento.Receita ? "receitas" : "despesas")} mensais de {lancamento.Valor:C} lançadas."
                    : $"{lancamento.Tipo.NomeExibicao()} de {lancamento.Valor:C} {situacao}.";
                return salvarENovo != null
                    ? RedirectToAction(nameof(Create), new { tipo = lancamento.Tipo })
                    : RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, erro);
        }

        await CarregarListasAsync(lancamento);
        return View(lancamento);
    }

    // GET: Financeiro/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var lancamento = await _financeiro.ObterLancamentoAsync(id.Value);
        if (lancamento == null) return NotFound();

        await CarregarListasAsync(lancamento);
        return View(lancamento);
    }

    // POST: Financeiro/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Tipo,Descricao,Valor,Data,Pago,DataPagamento,CategoriaId,FormaPagamento,LoteId,Observacoes")] LancamentoFinanceiro lancamento)
    {
        if (id != lancamento.Id) return NotFound();

        LimparValidacaoDeNavegacao();
        if (ModelState.IsValid)
        {
            var erro = await _financeiro.SalvarLancamentoAsync(lancamento);
            if (erro == null)
            {
                TempData["Sucesso"] = "Lançamento atualizado.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, erro);
        }

        await CarregarListasAsync(lancamento);
        return View(lancamento);
    }

    // GET: Financeiro/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var lancamento = await _financeiro.ObterLancamentoAsync(id.Value);
        if (lancamento == null) return NotFound();
        return View(lancamento);
    }

    // POST: Financeiro/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await _financeiro.RemoverLancamentoAsync(id))
            TempData["Sucesso"] = "Lançamento excluído.";
        else
            TempData["Erro"] = "Lançamento não encontrado.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Financeiro/Contas?tipo=Despesa
    public async Task<IActionResult> Contas(TipoLancamento? tipo)
    {
        ViewData["Tipo"] = tipo;
        return View(await _financeiro.ObterContasEmAbertoAsync(tipo, DateTime.Today));
    }

    // POST: Financeiro/MarcarPago/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarPago(int id, DateTime? dataPagamento, string? voltarPara)
    {
        var lancamento = await _financeiro.ObterLancamentoAsync(id);
        var erro = await _financeiro.MarcarComoPagoAsync(id, dataPagamento ?? DateTime.Today);
        if (erro == null)
            TempData["Sucesso"] = $"\"{lancamento!.Descricao}\" marcada como {(lancamento.Tipo == TipoLancamento.Receita ? "recebida" : "paga")}.";
        else
            TempData["Erro"] = erro;

        return !string.IsNullOrEmpty(voltarPara) && Url.IsLocalUrl(voltarPara)
            ? LocalRedirect(voltarPara)
            : RedirectToAction(nameof(Contas));
    }

    // GET: Financeiro/FluxoDeCaixa
    public async Task<IActionResult> FluxoDeCaixa()
    {
        return View(await _financeiro.ObterFluxoDeCaixaAsync(DateTime.Today));
    }

    private void LimparValidacaoDeNavegacao()
    {
        ModelState.Remove(nameof(LancamentoFinanceiro.Categoria));
        ModelState.Remove(nameof(LancamentoFinanceiro.Lote));
    }

    private async Task CarregarListasAsync(LancamentoFinanceiro lancamento)
    {
        // Categorias ativas + a do lançamento (mesmo se foi desativada depois)
        var categorias = await _financeiro.ObterCategoriasAsync();
        ViewData["Categorias"] = categorias.Where(c => c.Ativa || c.Id == lancamento.CategoriaId).ToList();

        var lotes = await _loteService.ObterTodosAsync();
        ViewData["Lotes"] = lotes.Where(l => l.Ativo || l.Id == lancamento.LoteId).OrderBy(l => l.CodigoLote).ToList();
    }
}
