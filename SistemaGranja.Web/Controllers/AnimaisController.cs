// Arquivo: AnimaisController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectList
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System.Linq;
using System.Threading.Tasks;

// [Authorize]
public class AnimaisController : Controller
{
    private readonly IAnimalService _animalService;
    private readonly IGalpaoService _galpaoService; // Para popular dropdown de Galpões
    private readonly ILoteService _loteService;     // Para popular dropdown de Lotes

    public AnimaisController(IAnimalService animalService, IGalpaoService galpaoService, ILoteService loteService)
    {
        _animalService = animalService;
        _galpaoService = galpaoService;
        _loteService = loteService;
    }

    // GET: Animais
    public async Task<IActionResult> Index()
    {
        var animais = await _animalService.ObterTodosComRelacionamentosAsync();
        return View(animais);
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
        // Prepara dados para os dropdowns (listas suspensas)
        ViewData["GalpaoId"] = new SelectList(await _galpaoService.ObterTodosAsync(), "Id", "NomeOuNumero");
        ViewData["LoteId"] = new SelectList(await _loteService.ObterTodosAsync(), "Id", "CodigoLote");
        return View();
    }

    // POST: Animais/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TagIdentificacao,Nome,Raca,DataNascimento,Sexo,PesoAtual,StatusSaude,GalpaoId,LoteId,DataEntradaGranja,MotivoEntrada,OrigemAnimal")] Animal animal)
    {
        // Remova Id da validação, pois é gerado pelo banco.
        ModelState.Remove("Id");
        // Remova propriedades de navegação da validação se não estiverem sendo preenchidas pelo form.
        ModelState.Remove("Galpao");
        ModelState.Remove("Lote");
        ModelState.Remove("RegistrosSaude");
        ModelState.Remove("RegistrosVacinacao");


        if (ModelState.IsValid)
        {
            await _animalService.AdicionarAsync(animal);
            return RedirectToAction(nameof(Index));
        }
        // Se inválido, precisa recarregar os dropdowns
        ViewData["GalpaoId"] = new SelectList(await _galpaoService.ObterTodosAsync(), "Id", "NomeOuNumero", animal.GalpaoId);
        ViewData["LoteId"] = new SelectList(await _loteService.ObterTodosAsync(), "Id", "CodigoLote", animal.LoteId);
        return View(animal);
    }

    // GET: Animais/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var animal = await _animalService.ObterPorIdComRelacionamentosAsync(id.Value);
        if (animal == null) return NotFound();

        ViewData["GalpaoId"] = new SelectList(await _galpaoService.ObterTodosAsync(), "Id", "NomeOuNumero", animal.GalpaoId);
        ViewData["LoteId"] = new SelectList(await _loteService.ObterTodosAsync(), "Id", "CodigoLote", animal.LoteId);
        return View(animal);
    }

    // POST: Animais/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,TagIdentificacao,Nome,Raca,DataNascimento,Sexo,PesoAtual,StatusSaude,GalpaoId,LoteId,DataEntradaGranja,MotivoEntrada,OrigemAnimal,DataSaidaGranja,MotivoSaida,DestinoAnimal,CausaMorte")] Animal animal)
    {
        if (id != animal.Id) return NotFound();

        ModelState.Remove("Galpao");
        ModelState.Remove("Lote");
        ModelState.Remove("RegistrosSaude");
        ModelState.Remove("RegistrosVacinacao");

        if (ModelState.IsValid)
        {
            try
            {
                await _animalService.AtualizarAsync(animal);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _animalService.ObterPorIdComRelacionamentosAsync(animal.Id) == null) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["GalpaoId"] = new SelectList(await _galpaoService.ObterTodosAsync(), "Id", "NomeOuNumero", animal.GalpaoId);
        ViewData["LoteId"] = new SelectList(await _loteService.ObterTodosAsync(), "Id", "CodigoLote", animal.LoteId);
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
        return RedirectToAction(nameof(Index));
    }
}