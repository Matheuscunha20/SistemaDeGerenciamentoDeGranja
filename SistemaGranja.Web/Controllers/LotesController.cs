using SistemaGranja.Web.Identidade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize]
public class LotesController : Controller
{
    private readonly ILoteService _loteService;
    private readonly IEspecieService _especieService;
    private readonly IGalpaoService _galpaoService;

    public LotesController(ILoteService loteService, IEspecieService especieService, IGalpaoService galpaoService)
    {
        _loteService = loteService;
        _especieService = especieService;
        _galpaoService = galpaoService;
    }

    // GET: Lotes?situacao=ativos|encerrados|todos
    public async Task<IActionResult> Index(string situacao = "ativos")
    {
        bool? ativo = situacao switch
        {
            "encerrados" => false,
            "todos" => null,
            _ => true
        };

        ViewData["Situacao"] = ativo switch { true => "ativos", false => "encerrados", null => "todos" };
        ViewData["QuantidadeAnimais"] = await _loteService.ObterQuantidadeAnimaisAsync();
        return View(await _loteService.ObterTodosAsync(ativo));
    }

    // GET: Lotes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var lote = await _loteService.ObterComAnimaisAsync(id.Value);
        if (lote == null) return NotFound();
        return View(lote);
    }

    // GET: Lotes/Create
    [Authorize(Policy = Politicas.Gestao)]
    public IActionResult Create()
    {
        return View(new Lote());
    }

    // POST: Lotes/Create
    [Authorize(Policy = Politicas.Gestao)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CodigoLote,DataCriacao,Descricao")] Lote lote)
    {
        ModelState.Remove(nameof(Lote.Animais));
        await ValidarAsync(lote);

        if (!ModelState.IsValid) return View(lote);

        lote.Ativo = true;
        await _loteService.AdicionarAsync(lote);
        TempData["Sucesso"] = $"Lote {lote.CodigoLote} criado. Agora adicione os animais.";
        return RedirectToAction(nameof(Details), new { id = lote.Id });
    }

    // GET: Lotes/Edit/5
    [Authorize(Policy = Politicas.Gestao)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var lote = await _loteService.ObterPorIdAsync(id.Value);
        if (lote == null) return NotFound();
        return View(lote);
    }

    // POST: Lotes/Edit/5
    [Authorize(Policy = Politicas.Gestao)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CodigoLote,DataCriacao,Descricao,Ativo")] Lote lote)
    {
        if (id != lote.Id) return NotFound();

        ModelState.Remove(nameof(Lote.Animais));
        await ValidarAsync(lote);

        if (!ModelState.IsValid) return View(lote);

        try
        {
            await _loteService.AtualizarAsync(lote);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (await _loteService.ObterPorIdAsync(lote.Id) == null) return NotFound();
            throw;
        }

        TempData["Sucesso"] = $"Lote {lote.CodigoLote} atualizado com sucesso.";
        return RedirectToAction(nameof(Details), new { id = lote.Id });
    }

    // POST: Lotes/AlterarSituacao/5 — encerra ou reabre o lote
    [Authorize(Policy = Politicas.Gestao)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSituacao(int id, bool ativo)
    {
        await _loteService.AlterarSituacaoAsync(id, ativo);
        TempData["Sucesso"] = ativo ? "Lote reaberto." : "Lote encerrado. Ele não aparece mais na escolha de lote dos animais.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Lotes/AdicionarAnimais/5 — lista animais que podem entrar no lote
    public async Task<IActionResult> AdicionarAnimais(int id, string? busca, int? especieId, int? galpaoId)
    {
        var lote = await _loteService.ObterPorIdAsync(id);
        if (lote == null) return NotFound();

        if (!lote.Ativo)
        {
            TempData["Erro"] = "Este lote está encerrado. Reabra o lote para adicionar animais.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewData["Lote"] = lote;
        ViewData["Busca"] = busca;
        ViewData["FiltroEspecies"] = new SelectList(await _especieService.ObterTodasAsync(), "Id", "Nome", especieId);
        ViewData["FiltroGalpoes"] = new SelectList(await _galpaoService.ObterTodosAsync(), "Id", "NomeOuNumero", galpaoId);

        return View(await _loteService.ObterAnimaisForaDoLoteAsync(id, busca, especieId, galpaoId));
    }

    // POST: Lotes/AdicionarAnimais/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarAnimais(int id, List<int> animaisSelecionados)
    {
        var lote = await _loteService.ObterPorIdAsync(id);
        if (lote == null) return NotFound();

        if (animaisSelecionados == null || animaisSelecionados.Count == 0)
        {
            TempData["Erro"] = "Nenhum animal foi selecionado.";
            return RedirectToAction(nameof(AdicionarAnimais), new { id });
        }

        var quantidade = await _loteService.AdicionarAnimaisAsync(id, animaisSelecionados);
        TempData["Sucesso"] = $"{quantidade} animal(is) adicionado(s) ao lote {lote.CodigoLote}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Lotes/RemoverAnimal/5?animalId=10 — tira o animal do lote (não exclui o animal)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverAnimal(int id, int animalId)
    {
        await _loteService.RemoverAnimalDoLoteAsync(id, animalId);
        TempData["Sucesso"] = "Animal retirado do lote.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Lotes/Delete/5
    [Authorize(Policy = Politicas.Gestao)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var lote = await _loteService.ObterComAnimaisAsync(id.Value);
        if (lote == null) return NotFound();
        return View(lote);
    }

    // POST: Lotes/Delete/5
    [Authorize(Policy = Politicas.Gestao)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _loteService.RemoverAsync(id))
        {
            TempData["Erro"] = "Este lote ainda tem animais. Retire os animais antes de excluir, ou apenas encerre o lote.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Sucesso"] = "Lote excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(Lote lote)
    {
        if (!string.IsNullOrWhiteSpace(lote.CodigoLote)
            && await _loteService.ExisteCodigoAsync(lote.CodigoLote, lote.Id == 0 ? null : lote.Id))
        {
            ModelState.AddModelError(nameof(Lote.CodigoLote), "Já existe um lote com este código.");
        }

        if (lote.DataCriacao > System.DateTime.Today)
        {
            ModelState.AddModelError(nameof(Lote.DataCriacao), "A data de criação não pode ser no futuro.");
        }
    }
}
