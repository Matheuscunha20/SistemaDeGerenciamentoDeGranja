using SistemaGranja.Web.Identidade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class VacinacoesController : Controller
{
    private readonly IVacinacaoService _vacinacaoService;
    private readonly ITipoVacinaService _tipoVacinaService;
    private readonly IAnimalService _animalService;
    private readonly IFuncionarioService _funcionarioService;
    private readonly IEspecieService _especieService;
    private readonly IGalpaoService _galpaoService;
    private readonly ILoteService _loteService;

    public VacinacoesController(
        IVacinacaoService vacinacaoService,
        ITipoVacinaService tipoVacinaService,
        IAnimalService animalService,
        IFuncionarioService funcionarioService,
        IEspecieService especieService,
        IGalpaoService galpaoService,
        ILoteService loteService)
    {
        _vacinacaoService = vacinacaoService;
        _tipoVacinaService = tipoVacinaService;
        _animalService = animalService;
        _funcionarioService = funcionarioService;
        _especieService = especieService;
        _galpaoService = galpaoService;
        _loteService = loteService;
    }

    // GET: Vacinacoes — agenda de próximas doses (atrasadas e dos próximos dias)
    public async Task<IActionResult> Index(int dias = 30)
    {
        if (dias is not (7 or 30 or 90)) dias = 30;
        ViewData["Dias"] = dias;
        return View(await _vacinacaoService.ObterProximasDosesAsync(dias));
    }

    // GET: Vacinacoes/Historico?busca=&tipoVacinaId=&de=&ate=
    public async Task<IActionResult> Historico(string? busca, int? tipoVacinaId, DateTime? de, DateTime? ate)
    {
        ViewData["Busca"] = busca;
        ViewData["De"] = de?.ToString("yyyy-MM-dd");
        ViewData["Ate"] = ate?.ToString("yyyy-MM-dd");
        ViewData["FiltroVacinas"] = new SelectList(await _tipoVacinaService.ObterTodosAsync(), "Id", "NomeVacina", tipoVacinaId);
        ViewData["FiltroAtivo"] = !string.IsNullOrWhiteSpace(busca) || tipoVacinaId.HasValue || de.HasValue || ate.HasValue;

        return View(await _vacinacaoService.ObterHistoricoAsync(null, tipoVacinaId, de, ate, busca));
    }

    // GET: Vacinacoes/Create?animalId=5&tipoVacinaId=2
    public async Task<IActionResult> Create(int? animalId, int? tipoVacinaId)
    {
        var registro = new RegistroVacinacao
        {
            AnimalId = animalId ?? 0,
            TipoVacinaId = tipoVacinaId ?? 0
        };

        if (tipoVacinaId.HasValue)
        {
            registro.DataProximaDoseRecomendada = await _vacinacaoService.CalcularProximaDoseAsync(tipoVacinaId.Value, registro.DataVacinacao);
            var tipo = await _tipoVacinaService.ObterPorIdAsync(tipoVacinaId.Value);
            if (tipo != null) registro.DoseAplicada = tipo.DosagemPadraoRecomendada;
        }

        await CarregarListasAsync(registro);
        ViewData["VoltarParaAnimal"] = animalId;
        return View(registro);
    }

    // POST: Vacinacoes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("AnimalId,TipoVacinaId,DataVacinacao,DoseAplicada,LoteProdutoVacina,ValidadeProdutoVacina,FuncionarioId,DataProximaDoseRecomendada,Observacoes")] RegistroVacinacao registro,
        int? voltarParaAnimal)
    {
        LimparValidacaoDeNavegacao();
        Validar(registro);

        if (!ModelState.IsValid)
        {
            await CarregarListasAsync(registro);
            ViewData["VoltarParaAnimal"] = voltarParaAnimal;
            return View(registro);
        }

        await _vacinacaoService.AdicionarAsync(registro);
        TempData["Sucesso"] = "Vacinação registrada com sucesso.";

        return voltarParaAnimal.HasValue
            ? RedirectToAction("Details", "Animais", new { id = voltarParaAnimal })
            : RedirectToAction(nameof(Historico));
    }

    // GET: Vacinacoes/Grupo — vacinar vários animais de uma vez
    public async Task<IActionResult> Grupo(string? busca, int? especieId, int? galpaoId, int? loteId)
    {
        var registro = new RegistroVacinacao();
        await CarregarListasAsync(registro);

        ViewData["Busca"] = busca;
        ViewData["FiltroEspecies"] = new SelectList(await _especieService.ObterTodasAsync(), "Id", "Nome", especieId);
        ViewData["FiltroGalpoes"] = new SelectList(await _galpaoService.ObterTodosAsync(), "Id", "NomeOuNumero", galpaoId);
        ViewData["FiltroLotes"] = new SelectList(await _loteService.ObterAtivosAsync(), "Id", "CodigoLote", loteId);

        var animais = await _animalService.ObterFiltradosAsync(busca, especieId, galpaoId, null);
        if (loteId.HasValue) animais = animais.Where(a => a.LoteId == loteId);
        ViewData["Animais"] = animais.ToList();

        return View(registro);
    }

    // POST: Vacinacoes/Grupo
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Grupo(
        [Bind("TipoVacinaId,DataVacinacao,DoseAplicada,LoteProdutoVacina,ValidadeProdutoVacina,FuncionarioId,DataProximaDoseRecomendada,Observacoes")] RegistroVacinacao modelo,
        List<int> animaisSelecionados)
    {
        // Os animais vêm das caixas de seleção, não do campo AnimalId
        ModelState.Remove(nameof(RegistroVacinacao.AnimalId));
        LimparValidacaoDeNavegacao();
        Validar(modelo);

        if (animaisSelecionados == null || animaisSelecionados.Count == 0)
            ModelState.AddModelError(string.Empty, "Selecione pelo menos um animal.");

        if (!ModelState.IsValid)
        {
            TempData["Erro"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Distinct());
            return RedirectToAction(nameof(Grupo));
        }

        var quantidade = await _vacinacaoService.AdicionarEmGrupoAsync(modelo, animaisSelecionados!);
        TempData["Sucesso"] = $"Vacinação registrada para {quantidade} animal(is).";
        return RedirectToAction(nameof(Historico));
    }

    // GET: Vacinacoes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var registro = await _vacinacaoService.ObterPorIdAsync(id.Value);
        if (registro == null) return NotFound();

        await CarregarListasAsync(registro);
        return View(registro);
    }

    // POST: Vacinacoes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,AnimalId,TipoVacinaId,DataVacinacao,DoseAplicada,LoteProdutoVacina,ValidadeProdutoVacina,FuncionarioId,DataProximaDoseRecomendada,Observacoes")] RegistroVacinacao registro)
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
            await _vacinacaoService.AtualizarAsync(registro);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (await _vacinacaoService.ObterPorIdAsync(registro.Id) == null) return NotFound();
            throw;
        }

        TempData["Sucesso"] = "Vacinação atualizada com sucesso.";
        return RedirectToAction(nameof(Historico));
    }

    // GET: Vacinacoes/Delete/5
    [Authorize(Policy = Politicas.Gestao)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var registro = await _vacinacaoService.ObterPorIdAsync(id.Value);
        if (registro == null) return NotFound();
        return View(registro);
    }

    // POST: Vacinacoes/Delete/5
    [Authorize(Policy = Politicas.Gestao)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _vacinacaoService.RemoverAsync(id);
        TempData["Sucesso"] = "Registro de vacinação excluído.";
        return RedirectToAction(nameof(Historico));
    }

    private void Validar(RegistroVacinacao registro)
    {
        if (registro.DataVacinacao.Date > DateTime.Today)
            ModelState.AddModelError(nameof(RegistroVacinacao.DataVacinacao), "A data da vacinação não pode ser no futuro.");

        if (registro.ValidadeProdutoVacina.HasValue && registro.ValidadeProdutoVacina.Value.Date < registro.DataVacinacao.Date)
            ModelState.AddModelError(nameof(RegistroVacinacao.ValidadeProdutoVacina), "O frasco estava vencido na data da vacinação.");

        if (registro.DataProximaDoseRecomendada.HasValue && registro.DataProximaDoseRecomendada.Value.Date <= registro.DataVacinacao.Date)
            ModelState.AddModelError(nameof(RegistroVacinacao.DataProximaDoseRecomendada), "A próxima dose deve ser depois da data da vacinação.");
    }

    private void LimparValidacaoDeNavegacao()
    {
        ModelState.Remove(nameof(RegistroVacinacao.Animal));
        ModelState.Remove(nameof(RegistroVacinacao.TipoVacina));
        ModelState.Remove(nameof(RegistroVacinacao.Funcionario));
    }

    // Listas do formulário. As vacinas vão com o intervalo de reforço e a dose padrão,
    // que o JavaScript usa para preencher a próxima dose automaticamente.
    private async Task CarregarListasAsync(RegistroVacinacao registro)
    {
        var animais = await _animalService.ObterTodosComRelacionamentosAsync();
        ViewData["Animais"] = new SelectList(
            animais.Select(a => new
            {
                a.Id,
                Texto = $"{a.TagIdentificacao}" +
                        (string.IsNullOrEmpty(a.Nome) ? "" : $" - {a.Nome}") +
                        (a.Raca?.Especie != null ? $" ({a.Raca.Especie.Nome})" : "")
            }),
            "Id", "Texto", registro.AnimalId);

        ViewData["Vacinas"] = (await _tipoVacinaService.ObterTodosAsync()).ToList();

        // Funcionários ativos, mais quem aplicou (caso já tenha sido desligado)
        var funcionarios = (await _funcionarioService.ObterTodosAsync(true)).ToList();
        if (registro.FuncionarioId.HasValue && funcionarios.All(f => f.Id != registro.FuncionarioId))
        {
            var quemAplicou = await _funcionarioService.ObterPorIdAsync(registro.FuncionarioId.Value);
            if (quemAplicou != null) funcionarios.Insert(0, quemAplicou);
        }
        ViewData["Funcionarios"] = new SelectList(funcionarios, "Id", "NomeCompleto", registro.FuncionarioId);
    }
}
