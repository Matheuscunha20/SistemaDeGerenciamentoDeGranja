using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class AnimaisController : Controller
{
    private readonly IAnimalService _animalService;
    private readonly IGalpaoService _galpaoService;
    private readonly ILoteService _loteService;
    private readonly IEspecieService _especieService;
    private readonly IRacaService _racaService;

    public static readonly string[] EstadosDeSaude = { "Saudável", "Em observação", "Em tratamento", "Em quarentena" };

    public AnimaisController(
        IAnimalService animalService,
        IGalpaoService galpaoService,
        ILoteService loteService,
        IEspecieService especieService,
        IRacaService racaService)
    {
        _animalService = animalService;
        _galpaoService = galpaoService;
        _loteService = loteService;
        _especieService = especieService;
        _racaService = racaService;
    }

    // GET: Animais?busca=&especieId=&galpaoId=&statusSaude=
    public async Task<IActionResult> Index(string? busca, int? especieId, int? galpaoId, string? statusSaude)
    {
        var animais = await _animalService.ObterFiltradosAsync(busca, especieId, galpaoId, statusSaude);

        ViewData["Busca"] = busca;
        ViewData["StatusSaude"] = statusSaude;
        ViewData["FiltroEspecies"] = new SelectList(await _especieService.ObterTodasAsync(), "Id", "Nome", especieId);
        ViewData["FiltroGalpoes"] = new SelectList(await _galpaoService.ObterTodosAsync(), "Id", "NomeOuNumero", galpaoId);
        ViewData["FiltroStatus"] = new SelectList(EstadosDeSaude, statusSaude);
        ViewData["FiltroAtivo"] = !string.IsNullOrWhiteSpace(busca) || especieId.HasValue || galpaoId.HasValue || !string.IsNullOrWhiteSpace(statusSaude);

        return View(animais);
    }

    // GET: Animais/RacasPorEspecie?especieId=1
    // Usado pelo formulário para atualizar a lista de raças quando a espécie muda
    [HttpGet]
    public async Task<IActionResult> RacasPorEspecie(int especieId)
    {
        var racas = await _racaService.ObterTodasAsync(especieId);
        return Json(racas.Select(r => new { id = r.Id, nome = r.Nome }));
    }

    // GET: Animais/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var animal = await _animalService.ObterPorIdComRelacionamentosAsync(id.Value);
        if (animal == null) return NotFound();
        return View(animal);
    }

    // GET: Animais/Create
    public async Task<IActionResult> Create()
    {
        var animal = new Animal();
        await CarregarListasAsync(animal, null);
        return View(animal);
    }

    // POST: Animais/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("TagIdentificacao,Nome,RacaId,DataNascimento,Sexo,PesoAtual,StatusSaude,GalpaoId,LoteId")] Animal animal,
        int? especieId)
    {
        LimparValidacaoDeNavegacao();
        await ValidarAsync(animal);

        if (ModelState.IsValid)
        {
            await _animalService.AdicionarAsync(animal);
            TempData["Sucesso"] = $"Animal {animal.TagIdentificacao} cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        await CarregarListasAsync(animal, especieId);
        return View(animal);
    }

    // GET: Animais/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var animal = await _animalService.ObterPorIdComRelacionamentosAsync(id.Value);
        if (animal == null) return NotFound();

        await CarregarListasAsync(animal, animal.Raca?.EspecieId);
        return View(animal);
    }

    // POST: Animais/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,TagIdentificacao,Nome,RacaId,DataNascimento,Sexo,PesoAtual,StatusSaude,GalpaoId,LoteId")] Animal animal,
        int? especieId)
    {
        if (id != animal.Id) return NotFound();

        LimparValidacaoDeNavegacao();
        await ValidarAsync(animal);

        if (ModelState.IsValid)
        {
            try
            {
                await _animalService.AtualizarAsync(animal);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _animalService.ObterPorIdComRelacionamentosAsync(animal.Id) == null) return NotFound();
                throw;
            }
            TempData["Sucesso"] = $"Animal {animal.TagIdentificacao} atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        await CarregarListasAsync(animal, especieId);
        return View(animal);
    }

    // GET: Animais/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var animal = await _animalService.ObterPorIdComRelacionamentosAsync(id.Value);
        if (animal == null) return NotFound();
        return View(animal);
    }

    // POST: Animais/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _animalService.RemoverAsync(id);
        TempData["Sucesso"] = "Animal excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // Regras que dependem do banco de dados
    private async Task ValidarAsync(Animal animal)
    {
        if (!string.IsNullOrWhiteSpace(animal.TagIdentificacao)
            && await _animalService.ExisteTagAsync(animal.TagIdentificacao, animal.Id == 0 ? null : animal.Id))
        {
            ModelState.AddModelError(nameof(Animal.TagIdentificacao), "Já existe um animal com este brinco/tag.");
        }

        if (animal.DataNascimento > System.DateTime.Today)
        {
            ModelState.AddModelError(nameof(Animal.DataNascimento), "A data de nascimento não pode ser no futuro.");
        }
    }

    // Os objetos relacionados não vêm do formulário, só os Ids
    private void LimparValidacaoDeNavegacao()
    {
        ModelState.Remove(nameof(Animal.Galpao));
        ModelState.Remove(nameof(Animal.Lote));
        ModelState.Remove(nameof(Animal.Raca));
        ModelState.Remove(nameof(Animal.RegistrosSaude));
        ModelState.Remove(nameof(Animal.RegistrosVacinacao));
    }

    // Prepara as listas suspensas do formulário
    private async Task CarregarListasAsync(Animal animal, int? especieId)
    {
        // Se a espécie não veio do formulário, descobre pela raça escolhida
        if (especieId == null && animal.RacaId.HasValue)
        {
            var raca = await _racaService.ObterPorIdAsync(animal.RacaId.Value);
            especieId = raca?.EspecieId;
        }

        var racas = especieId.HasValue
            ? await _racaService.ObterTodasAsync(especieId.Value)
            : Enumerable.Empty<Raca>();

        ViewData["EspecieId"] = new SelectList(await _especieService.ObterTodasAsync(), "Id", "Nome", especieId);
        ViewData["RacaId"] = new SelectList(racas, "Id", "Nome", animal.RacaId);
        ViewData["GalpaoId"] = new SelectList(await _galpaoService.ObterTodosAsync(), "Id", "NomeOuNumero", animal.GalpaoId);
        ViewData["LoteId"] = new SelectList(await _loteService.ObterTodosAsync(), "Id", "CodigoLote", animal.LoteId);
    }
}
