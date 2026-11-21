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
public class EstoqueController : Controller
{
    private readonly IEstoqueService _estoqueService;
    private readonly ITipoAlimentoService _tipoAlimentoService;
    private readonly IFuncionarioService _funcionarioService;

    public EstoqueController(IEstoqueService estoqueService, ITipoAlimentoService tipoAlimentoService, IFuncionarioService funcionarioService)
    {
        _estoqueService = estoqueService;
        _tipoAlimentoService = tipoAlimentoService;
        _funcionarioService = funcionarioService;
    }

    // GET: Estoque — saldo atual de cada alimento
    public async Task<IActionResult> Index()
    {
        ViewData["GastoUltimos30Dias"] = await _estoqueService.ObterGastoComComprasAsync(DateTime.Today.AddDays(-30), DateTime.Today);
        return View(await _estoqueService.ObterSaldosAsync());
    }

    // GET: Estoque/Movimentacoes?tipoAlimentoId=&tipo=&de=&ate=
    public async Task<IActionResult> Movimentacoes(int? tipoAlimentoId, TipoMovimentacaoEstoque? tipo, DateTime? de, DateTime? ate)
    {
        ViewData["Tipo"] = tipo;
        ViewData["De"] = de?.ToString("yyyy-MM-dd");
        ViewData["Ate"] = ate?.ToString("yyyy-MM-dd");
        ViewData["FiltroAlimentos"] = new SelectList(await _tipoAlimentoService.ObterTodosAsync(), "Id", "Nome", tipoAlimentoId);
        ViewData["FiltroAtivo"] = tipoAlimentoId.HasValue || tipo.HasValue || de.HasValue || ate.HasValue;

        return View(await _estoqueService.ObterMovimentacoesAsync(tipoAlimentoId, tipo, de, ate));
    }

    // GET: Estoque/Create?tipoAlimentoId=1&tipo=EntradaCompra
    public async Task<IActionResult> Create(int? tipoAlimentoId, TipoMovimentacaoEstoque? tipo)
    {
        var movimentacao = new MovimentacaoEstoqueAlimento
        {
            TipoAlimentoId = tipoAlimentoId ?? 0,
            TipoMovimentacao = tipo ?? TipoMovimentacaoEstoque.EntradaCompra
        };

        await CarregarListasAsync(movimentacao);
        return View(movimentacao);
    }

    // POST: Estoque/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("TipoAlimentoId,DataMovimentacao,TipoMovimentacao,Quantidade,PrecoUnitario,FornecedorOuDestino,NotaFiscal,FuncionarioId,Observacoes")] MovimentacaoEstoqueAlimento movimentacao)
    {
        LimparValidacaoDeNavegacao();
        await ValidarAsync(movimentacao);

        if (!ModelState.IsValid)
        {
            await CarregarListasAsync(movimentacao);
            return View(movimentacao);
        }

        await _estoqueService.AdicionarAsync(movimentacao);
        var saldo = await _estoqueService.ObterSaldoAsync(movimentacao.TipoAlimentoId);
        TempData["Sucesso"] = $"{movimentacao.TipoMovimentacao.NomeExibicao()} registrada. Saldo atual: {saldo:N2} {movimentacao.UnidadeMedida}.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Estoque/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var movimentacao = await _estoqueService.ObterPorIdAsync(id.Value);
        if (movimentacao == null) return NotFound();

        await CarregarListasAsync(movimentacao);
        return View(movimentacao);
    }

    // POST: Estoque/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,TipoAlimentoId,DataMovimentacao,TipoMovimentacao,Quantidade,PrecoUnitario,FornecedorOuDestino,NotaFiscal,FuncionarioId,Observacoes")] MovimentacaoEstoqueAlimento movimentacao)
    {
        if (id != movimentacao.Id) return NotFound();

        LimparValidacaoDeNavegacao();
        await ValidarAsync(movimentacao);

        if (!ModelState.IsValid)
        {
            await CarregarListasAsync(movimentacao);
            return View(movimentacao);
        }

        // Se trocou o alimento, o alimento antigo não pode ficar negativo
        var original = await _estoqueService.ObterPorIdAsync(id);
        if (original == null) return NotFound();
        if (original.TipoAlimentoId != movimentacao.TipoAlimentoId && original.TipoMovimentacao.EhEntrada())
        {
            var saldoAntigoSemEsta = await _estoqueService.ObterSaldoAsync(original.TipoAlimentoId, id);
            if (saldoAntigoSemEsta < 0)
            {
                ModelState.AddModelError(nameof(MovimentacaoEstoqueAlimento.TipoAlimentoId),
                    $"Não dá para trocar o alimento: o estoque de {original.TipoAlimento?.Nome} ficaria negativo.");
                await CarregarListasAsync(movimentacao);
                return View(movimentacao);
            }
        }

        try
        {
            await _estoqueService.AtualizarAsync(movimentacao);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (await _estoqueService.ObterPorIdAsync(movimentacao.Id) == null) return NotFound();
            throw;
        }

        TempData["Sucesso"] = "Movimentação atualizada.";
        return RedirectToAction(nameof(Movimentacoes));
    }

    // GET: Estoque/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var movimentacao = await _estoqueService.ObterPorIdAsync(id.Value);
        if (movimentacao == null) return NotFound();

        // Excluir uma entrada não pode deixar o saldo negativo
        var saldoSemEsta = await _estoqueService.ObterSaldoAsync(movimentacao.TipoAlimentoId, movimentacao.Id);
        ViewData["SaldoDepois"] = saldoSemEsta;
        return View(movimentacao);
    }

    // POST: Estoque/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movimentacao = await _estoqueService.ObterPorIdAsync(id);
        if (movimentacao == null) return RedirectToAction(nameof(Movimentacoes));

        var saldoSemEsta = await _estoqueService.ObterSaldoAsync(movimentacao.TipoAlimentoId, id);
        if (saldoSemEsta < 0)
        {
            TempData["Erro"] = "Esta entrada não pode ser excluída: o estoque ficaria negativo, porque parte dela já foi consumida.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        await _estoqueService.RemoverAsync(id);
        TempData["Sucesso"] = "Movimentação excluída.";
        return RedirectToAction(nameof(Movimentacoes));
    }

    private async Task ValidarAsync(MovimentacaoEstoqueAlimento movimentacao)
    {
        if (movimentacao.DataMovimentacao.Date > DateTime.Today)
            ModelState.AddModelError(nameof(MovimentacaoEstoqueAlimento.DataMovimentacao), "A data não pode ser no futuro.");

        if (movimentacao.TipoAlimentoId <= 0 || movimentacao.Quantidade <= 0)
            return;

        // O saldo final do alimento não pode ficar negativo
        var saldoSemEsta = await _estoqueService.ObterSaldoAsync(
            movimentacao.TipoAlimentoId, movimentacao.Id == 0 ? null : movimentacao.Id);
        var saldoDepois = saldoSemEsta + movimentacao.QuantidadeComSinal;

        if (saldoDepois < 0)
        {
            ModelState.AddModelError(nameof(MovimentacaoEstoqueAlimento.Quantidade),
                $"Estoque insuficiente. Saldo disponível: {Math.Max(saldoSemEsta, 0):N2}.");
        }
    }

    private void LimparValidacaoDeNavegacao()
    {
        ModelState.Remove(nameof(MovimentacaoEstoqueAlimento.TipoAlimento));
        ModelState.Remove(nameof(MovimentacaoEstoqueAlimento.Funcionario));
    }

    private async Task CarregarListasAsync(MovimentacaoEstoqueAlimento movimentacao)
    {
        // Alimentos com o saldo atual no texto, ex.: "Ração de engorda (1.250,00 Kg)"
        var saldos = await _estoqueService.ObterSaldosAsync();
        ViewData["Alimentos"] = saldos.ToList();

        var funcionarios = (await _funcionarioService.ObterTodosAsync(true)).ToList();
        if (movimentacao.FuncionarioId.HasValue && funcionarios.All(f => f.Id != movimentacao.FuncionarioId))
        {
            var quemRegistrou = await _funcionarioService.ObterPorIdAsync(movimentacao.FuncionarioId.Value);
            if (quemRegistrou != null) funcionarios.Insert(0, quemRegistrou);
        }
        ViewData["Funcionarios"] = new SelectList(funcionarios, "Id", "NomeCompleto", movimentacao.FuncionarioId);
    }
}
