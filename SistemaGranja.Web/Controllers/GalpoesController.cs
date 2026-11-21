using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;    // Para Galpao
using SistemaGranja.Services;  // Para IGalpaoService
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class GalpoesController : Controller
{
    private readonly IGalpaoService _galpaoService;

    public GalpoesController(IGalpaoService galpaoService) // Injeção de Dependência do Serviço
    {
        _galpaoService = galpaoService;
    }

    // GET: Galpoes
    public async Task<IActionResult> Index()
    {
        var galpoes = await _galpaoService.ObterTodosAsync();
        return View(galpoes); // Passa a lista de galpões para a View Index
    }

    // GET: Galpoes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var galpao = await _galpaoService.ObterPorIdAsync(id.Value);
        if (galpao == null) return NotFound();
        return View(galpao);
    }

    // GET: Galpoes/Create
    public IActionResult Create()
    {
        return View(new Galpao()); // Mostra o formulário de criação
    }

    // POST: Galpoes/Create
    [HttpPost]
    [ValidateAntiForgeryToken] // Ajuda a prevenir ataques CSRF
    public async Task<IActionResult> Create([Bind("NomeOuNumero,TipoInstalacao,CapacidadeMaxima,LocalizacaoDescricao,Observacoes")] Galpao galpao)
    {
        if (ModelState.IsValid) // Verifica se os dados do formulário são válidos
        {
            await _galpaoService.AdicionarAsync(galpao);
            TempData["Sucesso"] = $"Galpão {galpao.NomeOuNumero} cadastrado com sucesso.";
            return RedirectToAction(nameof(Index)); // Volta para a lista após criar
        }
        return View(galpao); // Se inválido, mostra o formulário novamente com os erros
    }

    // GET: Galpoes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var galpao = await _galpaoService.ObterPorIdAsync(id.Value);
        if (galpao == null) return NotFound();
        return View(galpao); // Mostra o formulário de edição com os dados do galpão
    }

    // POST: Galpoes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,NomeOuNumero,TipoInstalacao,CapacidadeMaxima,LocalizacaoDescricao,Observacoes")] Galpao galpao)
    {
        if (id != galpao.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _galpaoService.AtualizarAsync(galpao);
            }
            catch (DbUpdateConcurrencyException) // Trata concorrência se necessário
            {
                if (await _galpaoService.ObterPorIdAsync(galpao.Id) == null) return NotFound();
                else throw;
            }
            TempData["Sucesso"] = $"Galpão {galpao.NomeOuNumero} atualizado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        return View(galpao);
    }

    // GET: Galpoes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var galpao = await _galpaoService.ObterPorIdAsync(id.Value);
        if (galpao == null) return NotFound();
        return View(galpao); // Mostra uma página de confirmação de exclusão
    }

    // POST: Galpoes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _galpaoService.RemoverAsync(id);
        TempData["Sucesso"] = "Galpão excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
