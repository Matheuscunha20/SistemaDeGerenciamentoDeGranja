using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using SistemaGranja.Web.Identidade;

// Categorias de receitas e despesas. Só Dono e Gerente.
[Authorize(Policy = Politicas.Gestao)]
public class CategoriasFinanceirasController : Controller
{
    private readonly IFinanceiroService _financeiro;

    public CategoriasFinanceirasController(IFinanceiroService financeiro)
    {
        _financeiro = financeiro;
    }

    // GET: CategoriasFinanceiras
    public async Task<IActionResult> Index()
    {
        await _financeiro.GarantirCategoriasPadraoAsync();
        return View(await _financeiro.ObterCategoriasAsync());
    }

    // GET: CategoriasFinanceiras/Create?tipo=Receita
    public IActionResult Create(TipoLancamento? tipo)
    {
        return View(new CategoriaFinanceira { Tipo = tipo ?? TipoLancamento.Despesa });
    }

    // POST: CategoriasFinanceiras/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Tipo")] CategoriaFinanceira categoria)
    {
        if (ModelState.IsValid)
        {
            categoria.Ativa = true;
            var erro = await _financeiro.SalvarCategoriaAsync(categoria);
            if (erro == null)
            {
                TempData["Sucesso"] = $"Categoria {categoria.Nome} criada.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(nameof(CategoriaFinanceira.Nome), erro);
        }
        return View(categoria);
    }

    // GET: CategoriasFinanceiras/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var categoria = await _financeiro.ObterCategoriaAsync(id.Value);
        if (categoria == null) return NotFound();
        return View(categoria);
    }

    // POST: CategoriasFinanceiras/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Tipo,Ativa")] CategoriaFinanceira categoria)
    {
        if (id != categoria.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var erro = await _financeiro.SalvarCategoriaAsync(categoria);
            if (erro == null)
            {
                TempData["Sucesso"] = $"Categoria {categoria.Nome} atualizada.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, erro);
        }
        return View(categoria);
    }

    // GET: CategoriasFinanceiras/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var categoria = await _financeiro.ObterCategoriaAsync(id.Value);
        if (categoria == null) return NotFound();

        ViewData["Usos"] = await _financeiro.ContarLancamentosDaCategoriaAsync(categoria.Id);
        return View(categoria);
    }

    // POST: CategoriasFinanceiras/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var erro = await _financeiro.RemoverCategoriaAsync(id);
        if (erro == null) TempData["Sucesso"] = "Categoria excluída.";
        else TempData["Erro"] = erro;
        return RedirectToAction(nameof(Index));
    }
}
