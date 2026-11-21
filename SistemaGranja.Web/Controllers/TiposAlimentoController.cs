// Arquivo: TiposAlimentoController.cs (na pasta Controllers)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Para DbUpdateConcurrencyException
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class TiposAlimentoController : Controller
{
    private readonly ITipoAlimentoService _tipoAlimentoService;

    public TiposAlimentoController(ITipoAlimentoService tipoAlimentoService) // Injeção de Dependência
    {
        _tipoAlimentoService = tipoAlimentoService;
    }

    // GET: TiposAlimento
    public async Task<IActionResult> Index()
    {
        var tiposAlimento = await _tipoAlimentoService.ObterTodosAsync();
        return View(tiposAlimento);
    }

    // GET: TiposAlimento/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var tipoAlimento = await _tipoAlimentoService.ObterPorIdAsync(id.Value);
        if (tipoAlimento == null) return NotFound();
        return View(tipoAlimento);
    }

    // GET: TiposAlimento/Create
    public IActionResult Create()
    {
        return View(new TipoAlimento());
    }

    // POST: TiposAlimento/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Descricao,UnidadeMedidaPadrao,Fabricante,CodigoInterno,EstoqueMinimo")] TipoAlimento tipoAlimento)
    {
        ModelState.Remove(nameof(TipoAlimento.MovimentacoesEstoque));
        await ValidarAsync(tipoAlimento);

        if (ModelState.IsValid)
        {
            await _tipoAlimentoService.AdicionarAsync(tipoAlimento);
            TempData["Sucesso"] = $"Tipo de alimento {tipoAlimento.Nome} cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        return View(tipoAlimento);
    }

    // GET: TiposAlimento/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var tipoAlimento = await _tipoAlimentoService.ObterPorIdAsync(id.Value);
        if (tipoAlimento == null) return NotFound();
        return View(tipoAlimento);
    }

    // POST: TiposAlimento/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,UnidadeMedidaPadrao,Fabricante,CodigoInterno,EstoqueMinimo")] TipoAlimento tipoAlimento)
    {
        if (id != tipoAlimento.Id) return NotFound();

        ModelState.Remove(nameof(TipoAlimento.MovimentacoesEstoque));
        await ValidarAsync(tipoAlimento);

        if (ModelState.IsValid)
        {
            try
            {
                await _tipoAlimentoService.AtualizarAsync(tipoAlimento);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _tipoAlimentoService.ObterPorIdAsync(tipoAlimento.Id) == null) return NotFound();
                else throw;
            }
            TempData["Sucesso"] = $"Tipo de alimento {tipoAlimento.Nome} atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        return View(tipoAlimento);
    }

    // GET: TiposAlimento/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var tipoAlimento = await _tipoAlimentoService.ObterPorIdAsync(id.Value);
        if (tipoAlimento == null) return NotFound();

        ViewData["QuantidadeMovimentacoes"] = await _tipoAlimentoService.ContarMovimentacoesAsync(tipoAlimento.Id);
        return View(tipoAlimento);
    }

    // POST: TiposAlimento/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _tipoAlimentoService.RemoverAsync(id))
        {
            TempData["Erro"] = "Este alimento tem movimentações de estoque e não pode ser excluído, para não perder o histórico.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Sucesso"] = "Tipo de alimento excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(TipoAlimento tipoAlimento)
    {
        if (!string.IsNullOrWhiteSpace(tipoAlimento.Nome)
            && await _tipoAlimentoService.ExisteNomeAsync(tipoAlimento.Nome, tipoAlimento.Id == 0 ? null : tipoAlimento.Id))
        {
            ModelState.AddModelError(nameof(TipoAlimento.Nome), "Já existe um tipo de alimento com este nome.");
        }
    }
}