using SistemaGranja.Web.Identidade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System.Threading.Tasks;

// Cadastros base: só Dono e Gerente
[Authorize(Policy = Politicas.Gestao)]
public class EspeciesController : Controller
{
    private readonly IEspecieService _especieService;

    public EspeciesController(IEspecieService especieService)
    {
        _especieService = especieService;
    }

    // GET: Especies
    public async Task<IActionResult> Index()
    {
        return View(await _especieService.ObterTodasAsync());
    }

    // GET: Especies/Create
    public IActionResult Create()
    {
        return View(new Especie());
    }

    // POST: Especies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome")] Especie especie)
    {
        ModelState.Remove(nameof(Especie.Racas));
        await ValidarAsync(especie);

        if (!ModelState.IsValid) return View(especie);

        await _especieService.AdicionarAsync(especie);
        TempData["Sucesso"] = $"Espécie {especie.Nome} cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Especies/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var especie = await _especieService.ObterPorIdAsync(id.Value);
        if (especie == null) return NotFound();
        return View(especie);
    }

    // POST: Especies/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] Especie especie)
    {
        if (id != especie.Id) return NotFound();

        ModelState.Remove(nameof(Especie.Racas));
        await ValidarAsync(especie);

        if (!ModelState.IsValid) return View(especie);

        await _especieService.AtualizarAsync(especie);
        TempData["Sucesso"] = $"Espécie {especie.Nome} atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Especies/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var especie = await _especieService.ObterPorIdAsync(id.Value);
        if (especie == null) return NotFound();
        return View(especie);
    }

    // POST: Especies/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _especieService.RemoverAsync(id))
        {
            TempData["Erro"] = "Esta espécie ainda tem raças cadastradas. Exclua as raças primeiro.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Sucesso"] = "Espécie excluída com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(Especie especie)
    {
        if (!string.IsNullOrWhiteSpace(especie.Nome)
            && await _especieService.ExisteNomeAsync(especie.Nome, especie.Id == 0 ? null : especie.Id))
        {
            ModelState.AddModelError(nameof(Especie.Nome), "Já existe uma espécie com este nome.");
        }
    }
}
