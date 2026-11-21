using SistemaGranja.Web.Identidade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System.Threading.Tasks;

[Authorize]
public class GalpoesController : Controller
{
    private readonly IGalpaoService _galpaoService;

    public GalpoesController(IGalpaoService galpaoService)
    {
        _galpaoService = galpaoService;
    }

    // GET: Galpoes
    public async Task<IActionResult> Index()
    {
        ViewData["Ocupacao"] = await _galpaoService.ObterOcupacaoAsync();
        return View(await _galpaoService.ObterTodosAsync());
    }

    // GET: Galpoes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var galpao = await _galpaoService.ObterComAnimaisAsync(id.Value);
        if (galpao == null) return NotFound();
        return View(galpao);
    }

    // GET: Galpoes/Create
    [Authorize(Policy = Politicas.Gestao)]
    public IActionResult Create()
    {
        return View(new Galpao());
    }

    // POST: Galpoes/Create
    [Authorize(Policy = Politicas.Gestao)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NomeOuNumero,TipoInstalacao,CapacidadeMaxima,LocalizacaoDescricao,Observacoes")] Galpao galpao)
    {
        ModelState.Remove(nameof(Galpao.Animais));
        await ValidarAsync(galpao);

        if (!ModelState.IsValid) return View(galpao);

        await _galpaoService.AdicionarAsync(galpao);
        TempData["Sucesso"] = $"Galpão {galpao.NomeOuNumero} cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Galpoes/Edit/5
    [Authorize(Policy = Politicas.Gestao)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var galpao = await _galpaoService.ObterPorIdAsync(id.Value);
        if (galpao == null) return NotFound();

        ViewData["Ocupados"] = await _galpaoService.ContarAnimaisAsync(galpao.Id);
        return View(galpao);
    }

    // POST: Galpoes/Edit/5
    [Authorize(Policy = Politicas.Gestao)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,NomeOuNumero,TipoInstalacao,CapacidadeMaxima,LocalizacaoDescricao,Observacoes")] Galpao galpao)
    {
        if (id != galpao.Id) return NotFound();

        ModelState.Remove(nameof(Galpao.Animais));
        await ValidarAsync(galpao);

        if (ModelState.IsValid)
        {
            try
            {
                await _galpaoService.AtualizarAsync(galpao);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _galpaoService.ObterPorIdAsync(galpao.Id) == null) return NotFound();
                throw;
            }
            TempData["Sucesso"] = $"Galpão {galpao.NomeOuNumero} atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Ocupados"] = await _galpaoService.ContarAnimaisAsync(galpao.Id);
        return View(galpao);
    }

    // GET: Galpoes/Delete/5
    [Authorize(Policy = Politicas.Gestao)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var galpao = await _galpaoService.ObterPorIdAsync(id.Value);
        if (galpao == null) return NotFound();

        ViewData["Ocupados"] = await _galpaoService.ContarAnimaisAsync(galpao.Id);
        return View(galpao);
    }

    // POST: Galpoes/Delete/5
    [Authorize(Policy = Politicas.Gestao)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _galpaoService.RemoverAsync(id))
        {
            TempData["Erro"] = "Este galpão ainda tem animais. Mova os animais para outro galpão antes de excluir.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Sucesso"] = "Galpão excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(Galpao galpao)
    {
        var idAtual = galpao.Id == 0 ? (int?)null : galpao.Id;

        if (!string.IsNullOrWhiteSpace(galpao.NomeOuNumero)
            && await _galpaoService.ExisteNomeAsync(galpao.NomeOuNumero, idAtual))
        {
            ModelState.AddModelError(nameof(Galpao.NomeOuNumero), "Já existe um galpão com este nome ou número.");
        }

        // Não deixa reduzir a capacidade abaixo do número de animais que já estão lá
        if (idAtual.HasValue)
        {
            var ocupados = await _galpaoService.ContarAnimaisAsync(idAtual.Value);
            if (galpao.CapacidadeMaxima < ocupados)
            {
                ModelState.AddModelError(nameof(Galpao.CapacidadeMaxima),
                    $"Este galpão já tem {ocupados} animal(is). A capacidade não pode ser menor que isso.");
            }
        }
    }
}
