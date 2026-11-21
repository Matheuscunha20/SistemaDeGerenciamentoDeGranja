using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class SaudeController : Controller
{
    private readonly ISaudeService _saudeService;
    private readonly IAnimalService _animalService;
    private readonly IFuncionarioService _funcionarioService;

    public SaudeController(ISaudeService saudeService, IAnimalService animalService, IFuncionarioService funcionarioService)
    {
        _saudeService = saudeService;
        _animalService = animalService;
        _funcionarioService = funcionarioService;
    }

    // GET: Saude?busca=&tipo=&de=&ate=
    public async Task<IActionResult> Index(string? busca, TipoOcorrenciaSaude? tipo, DateTime? de, DateTime? ate)
    {
        ViewData["Busca"] = busca;
        ViewData["Tipo"] = tipo;
        ViewData["De"] = de?.ToString("yyyy-MM-dd");
        ViewData["Ate"] = ate?.ToString("yyyy-MM-dd");
        ViewData["FiltroAtivo"] = !string.IsNullOrWhiteSpace(busca) || tipo.HasValue || de.HasValue || ate.HasValue;
        ViewData["AnimaisPorEstado"] = await _saudeService.ContarAnimaisPorEstadoAsync();

        return View(await _saudeService.ObterHistoricoAsync(null, tipo, de, ate, busca));
    }

    // GET: Saude/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var registro = await _saudeService.ObterPorIdAsync(id.Value);
        if (registro == null) return NotFound();
        return View(registro);
    }

    // GET: Saude/Create?animalId=5
    public async Task<IActionResult> Create(int? animalId)
    {
        var registro = new RegistroSaude { AnimalId = animalId ?? 0 };
        await CarregarListasAsync(registro);
        ViewData["VoltarParaAnimal"] = animalId;
        return View(registro);
    }

    // POST: Saude/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("AnimalId,DataOcorrencia,TipoOcorrencia,DescricaoDetalhada,Diagnostico,TratamentoRealizado,MedicamentosAdministrados,FuncionarioId,ObservacoesAdicionais")] RegistroSaude registro,
        string? novoEstadoDoAnimal,
        int? voltarParaAnimal)
    {
        LimparValidacaoDeNavegacao();
        Validar(registro);

        if (!string.IsNullOrWhiteSpace(novoEstadoDoAnimal) && !AnimaisController.EstadosDeSaude.Contains(novoEstadoDoAnimal))
            ModelState.AddModelError(string.Empty, "Estado de saúde inválido.");

        if (!ModelState.IsValid)
        {
            await CarregarListasAsync(registro);
            ViewData["VoltarParaAnimal"] = voltarParaAnimal;
            ViewData["NovoEstado"] = novoEstadoDoAnimal;
            return View(registro);
        }

        await _saudeService.AdicionarAsync(registro, novoEstadoDoAnimal);
        TempData["Sucesso"] = string.IsNullOrWhiteSpace(novoEstadoDoAnimal)
            ? "Ocorrência de saúde registrada."
            : $"Ocorrência registrada e animal marcado como \"{novoEstadoDoAnimal}\".";

        return voltarParaAnimal.HasValue
            ? RedirectToAction("Details", "Animais", new { id = voltarParaAnimal })
            : RedirectToAction(nameof(Index));
    }

    // GET: Saude/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var registro = await _saudeService.ObterPorIdAsync(id.Value);
        if (registro == null) return NotFound();

        await CarregarListasAsync(registro);
        return View(registro);
    }

    // POST: Saude/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,AnimalId,DataOcorrencia,TipoOcorrencia,DescricaoDetalhada,Diagnostico,TratamentoRealizado,MedicamentosAdministrados,FuncionarioId,ObservacoesAdicionais")] RegistroSaude registro)
    {
        if (id != registro.Id) return NotFound();

        LimparValidacaoDeNavegacao();
        Validar(registro);

        if (!ModelState.IsValid)
        {
            await CarregarListasAsync(registro);
            return View(registro);
        }

        try
        {
            await _saudeService.AtualizarAsync(registro);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (await _saudeService.ObterPorIdAsync(registro.Id) == null) return NotFound();
            throw;
        }

        TempData["Sucesso"] = "Ocorrência atualizada.";
        return RedirectToAction(nameof(Details), new { id = registro.Id });
    }

    // GET: Saude/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var registro = await _saudeService.ObterPorIdAsync(id.Value);
        if (registro == null) return NotFound();
        return View(registro);
    }

    // POST: Saude/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _saudeService.RemoverAsync(id);
        TempData["Sucesso"] = "Ocorrência excluída.";
        return RedirectToAction(nameof(Index));
    }

    private void Validar(RegistroSaude registro)
    {
        if (registro.DataOcorrencia.Date > DateTime.Today)
            ModelState.AddModelError(nameof(RegistroSaude.DataOcorrencia), "A data não pode ser no futuro.");
    }

    private void LimparValidacaoDeNavegacao()
    {
        ModelState.Remove(nameof(RegistroSaude.Animal));
        ModelState.Remove(nameof(RegistroSaude.Funcionario));
    }

    private async Task CarregarListasAsync(RegistroSaude registro)
    {
        var animais = await _animalService.ObterTodosComRelacionamentosAsync();
        ViewData["Animais"] = new SelectList(
            animais.Select(a => new
            {
                a.Id,
                Texto = $"{a.TagIdentificacao}" +
                        (string.IsNullOrEmpty(a.Nome) ? "" : $" - {a.Nome}") +
                        (a.Raca?.Especie != null ? $" ({a.Raca.Especie.Nome})" : "") +
                        $" · {a.StatusSaude}"
            }),
            "Id", "Texto", registro.AnimalId);

        // Funcionários ativos, mais quem atendeu (caso já tenha sido desligado)
        var funcionarios = (await _funcionarioService.ObterTodosAsync(true)).ToList();
        if (registro.FuncionarioId.HasValue && funcionarios.All(f => f.Id != registro.FuncionarioId))
        {
            var quemAtendeu = await _funcionarioService.ObterPorIdAsync(registro.FuncionarioId.Value);
            if (quemAtendeu != null) funcionarios.Insert(0, quemAtendeu);
        }
        ViewData["Funcionarios"] = new SelectList(funcionarios, "Id", "NomeCompleto", registro.FuncionarioId);
    }
}
