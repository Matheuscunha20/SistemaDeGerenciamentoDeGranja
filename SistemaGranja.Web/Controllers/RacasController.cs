using SistemaGranja.Web.Identidade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System.Threading.Tasks;

// Cadastros base: só Dono e Gerente
[Authorize(Policy = Politicas.Gestao)]
public class RacasController : Controller
{
    private readonly IRacaService _racaService;
    private readonly IEspecieService _especieService;

    public RacasController(IRacaService racaService, IEspecieService especieService)
    {
        _racaService = racaService;
        _especieService = especieService;
    }

    // GET: Racas?especieId=1
    public async Task<IActionResult> Index(int? especieId)
    {
        ViewData["FiltroEspecies"] = new SelectList(await _especieService.ObterTodasAsync(), "Id", "Nome", especieId);
        ViewData["EspecieFiltrada"] = especieId;
        return View(await _racaService.ObterTodasAsync(especieId));
    }

    // GET: Racas/Create?especieId=1
    public async Task<IActionResult> Create(int? especieId)
    {
        var raca = new Raca { EspecieId = especieId };
        await CarregarEspeciesAsync(raca.EspecieId);
        return View(raca);
    }

    // POST: Racas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,EspecieId")] Raca raca)
    {
        LimparValidacaoDeNavegacao();
        await ValidarAsync(raca);

        if (!ModelState.IsValid)
        {
            await CarregarEspeciesAsync(raca.EspecieId);
            return View(raca);
        }

        await _racaService.AdicionarAsync(raca);
        TempData["Sucesso"] = $"Raça {raca.Nome} cadastrada com sucesso.";
        return RedirectToAction(nameof(Index), new { especieId = raca.EspecieId });
    }

    // GET: Racas/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var raca = await _racaService.ObterPorIdAsync(id.Value);
        if (raca == null) return NotFound();

        await CarregarEspeciesAsync(raca.EspecieId);
        return View(raca);
    }

    // POST: Racas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,EspecieId")] Raca raca)
    {
        if (id != raca.Id) return NotFound();

        LimparValidacaoDeNavegacao();
        await ValidarAsync(raca);

        if (!ModelState.IsValid)
        {
            await CarregarEspeciesAsync(raca.EspecieId);
            return View(raca);
        }

        await _racaService.AtualizarAsync(raca);
        TempData["Sucesso"] = $"Raça {raca.Nome} atualizada com sucesso.";
        return RedirectToAction(nameof(Index), new { especieId = raca.EspecieId });
    }

    // GET: Racas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var raca = await _racaService.ObterPorIdAsync(id.Value);
        if (raca == null) return NotFound();

        ViewData["QuantidadeAnimais"] = await _racaService.ContarAnimaisAsync(raca.Id);
        return View(raca);
    }

    // POST: Racas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var raca = await _racaService.ObterPorIdAsync(id);

        if (!await _racaService.RemoverAsync(id))
        {
            TempData["Erro"] = "Esta raça ainda tem animais cadastrados. Altere a raça desses animais primeiro.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Sucesso"] = "Raça excluída com sucesso.";
        return RedirectToAction(nameof(Index), new { especieId = raca?.EspecieId });
    }

    private async Task ValidarAsync(Raca raca)
    {
        if (raca.EspecieId.HasValue && !string.IsNullOrWhiteSpace(raca.Nome)
            && await _racaService.ExisteNomeAsync(raca.EspecieId.Value, raca.Nome, raca.Id == 0 ? null : raca.Id))
        {
            ModelState.AddModelError(nameof(Raca.Nome), "Esta espécie já tem uma raça com este nome.");
        }
    }

    private void LimparValidacaoDeNavegacao()
    {
        ModelState.Remove(nameof(Raca.Especie));
        ModelState.Remove(nameof(Raca.Animais));
    }

    private async Task CarregarEspeciesAsync(int? especieSelecionada)
    {
        ViewData["Especies"] = new SelectList(await _especieService.ObterTodasAsync(), "Id", "Nome", especieSelecionada);
    }
}
