using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System.Threading.Tasks;

[Authorize]
public class TiposVacinaController : Controller
{
    private readonly ITipoVacinaService _tipoVacinaService;

    public TiposVacinaController(ITipoVacinaService tipoVacinaService)
    {
        _tipoVacinaService = tipoVacinaService;
    }

    // GET: TiposVacina
    public async Task<IActionResult> Index()
    {
        ViewData["Aplicacoes"] = await _tipoVacinaService.ObterQuantidadeAplicacoesAsync();
        return View(await _tipoVacinaService.ObterTodosAsync());
    }

    // GET: TiposVacina/Create
    public IActionResult Create()
    {
        return View(new TipoVacina());
    }

    // POST: TiposVacina/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NomeVacina,DoencaAlvo,Fabricante,DosagemPadraoRecomendada,ViaAdministracaoComum,IntervaloReforcoDias")] TipoVacina tipoVacina)
    {
        ModelState.Remove(nameof(TipoVacina.RegistrosVacinacao));
        await ValidarAsync(tipoVacina);

        if (!ModelState.IsValid) return View(tipoVacina);

        await _tipoVacinaService.AdicionarAsync(tipoVacina);
        TempData["Sucesso"] = $"Vacina {tipoVacina.NomeVacina} cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: TiposVacina/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var tipoVacina = await _tipoVacinaService.ObterPorIdAsync(id.Value);
        if (tipoVacina == null) return NotFound();
        return View(tipoVacina);
    }

    // POST: TiposVacina/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,NomeVacina,DoencaAlvo,Fabricante,DosagemPadraoRecomendada,ViaAdministracaoComum,IntervaloReforcoDias")] TipoVacina tipoVacina)
    {
        if (id != tipoVacina.Id) return NotFound();

        ModelState.Remove(nameof(TipoVacina.RegistrosVacinacao));
        await ValidarAsync(tipoVacina);

        if (!ModelState.IsValid) return View(tipoVacina);

        await _tipoVacinaService.AtualizarAsync(tipoVacina);
        TempData["Sucesso"] = $"Vacina {tipoVacina.NomeVacina} atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: TiposVacina/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var tipoVacina = await _tipoVacinaService.ObterPorIdAsync(id.Value);
        if (tipoVacina == null) return NotFound();

        var aplicacoes = await _tipoVacinaService.ObterQuantidadeAplicacoesAsync();
        ViewData["QuantidadeAplicacoes"] = aplicacoes.GetValueOrDefault(tipoVacina.Id);
        return View(tipoVacina);
    }

    // POST: TiposVacina/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _tipoVacinaService.RemoverAsync(id))
        {
            TempData["Erro"] = "Esta vacina já foi aplicada e não pode ser excluída, para não apagar o histórico.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Sucesso"] = "Vacina excluída com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(TipoVacina tipoVacina)
    {
        if (!string.IsNullOrWhiteSpace(tipoVacina.NomeVacina)
            && await _tipoVacinaService.ExisteNomeAsync(tipoVacina.NomeVacina, tipoVacina.Id == 0 ? null : tipoVacina.Id))
        {
            ModelState.AddModelError(nameof(TipoVacina.NomeVacina), "Já existe uma vacina com este nome.");
        }
    }
}
