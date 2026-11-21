using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using System;
using System.Threading.Tasks;

[Authorize]
public class FuncionariosController : Controller
{
    private readonly IFuncionarioService _funcionarioService;

    public static readonly string[] Cargos =
    {
        "Gerente", "Veterinário", "Zootecnista", "Técnico agropecuário", "Tratador",
        "Vaqueiro", "Ordenhador", "Auxiliar de campo", "Operador de máquinas", "Administrativo", "Outro"
    };

    public FuncionariosController(IFuncionarioService funcionarioService)
    {
        _funcionarioService = funcionarioService;
    }

    // GET: Funcionarios?situacao=ativos|desligados|todos&busca=
    public async Task<IActionResult> Index(string situacao = "ativos", string? busca = null)
    {
        bool? ativo = situacao switch
        {
            "desligados" => false,
            "todos" => null,
            _ => true
        };

        ViewData["Situacao"] = ativo switch { true => "ativos", false => "desligados", null => "todos" };
        ViewData["Busca"] = busca;
        return View(await _funcionarioService.ObterTodosAsync(ativo, busca));
    }

    // GET: Funcionarios/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var funcionario = await _funcionarioService.ObterPorIdAsync(id.Value);
        if (funcionario == null) return NotFound();
        return View(funcionario);
    }

    // GET: Funcionarios/Create
    public IActionResult Create()
    {
        return View(new Funcionario());
    }

    // POST: Funcionarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NomeCompleto,Cpf,Rg,Cargo,DataAdmissao,Telefone,Email")] Funcionario funcionario)
    {
        LimparValidacaoDeNavegacao();
        await ValidarAsync(funcionario);

        if (!ModelState.IsValid) return View(funcionario);

        await _funcionarioService.AdicionarAsync(funcionario);
        TempData["Sucesso"] = $"Funcionário {funcionario.NomeCompleto} cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Funcionarios/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var funcionario = await _funcionarioService.ObterPorIdAsync(id.Value);
        if (funcionario == null) return NotFound();

        // Mostra o CPF formatado no formulário
        funcionario.Cpf = funcionario.CpfFormatado;
        return View(funcionario);
    }

    // POST: Funcionarios/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,NomeCompleto,Cpf,Rg,Cargo,DataAdmissao,DataDemissao,Telefone,Email,Ativo")] Funcionario funcionario)
    {
        if (id != funcionario.Id) return NotFound();

        LimparValidacaoDeNavegacao();
        await ValidarAsync(funcionario);

        if (!ModelState.IsValid) return View(funcionario);

        try
        {
            await _funcionarioService.AtualizarAsync(funcionario);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (await _funcionarioService.ObterPorIdAsync(funcionario.Id) == null) return NotFound();
            throw;
        }

        TempData["Sucesso"] = $"Funcionário {funcionario.NomeCompleto} atualizado com sucesso.";
        return RedirectToAction(nameof(Details), new { id = funcionario.Id });
    }

    // POST: Funcionarios/Desligar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desligar(int id, DateTime? dataDesligamento)
    {
        var funcionario = await _funcionarioService.ObterPorIdAsync(id);
        if (funcionario == null) return NotFound();

        var data = dataDesligamento ?? DateTime.Today;
        if (data < funcionario.DataAdmissao.Date)
        {
            TempData["Erro"] = "A data de desligamento não pode ser antes da admissão.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _funcionarioService.DesligarAsync(id, data);
        TempData["Sucesso"] = $"{funcionario.NomeCompleto} foi desligado(a). O histórico de registros foi mantido.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Funcionarios/Reativar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id)
    {
        await _funcionarioService.ReativarAsync(id);
        TempData["Sucesso"] = "Funcionário reativado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Funcionarios/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var funcionario = await _funcionarioService.ObterPorIdAsync(id.Value);
        if (funcionario == null) return NotFound();

        ViewData["QuantidadeRegistros"] = await _funcionarioService.ContarRegistrosAsync(funcionario.Id);
        return View(funcionario);
    }

    // POST: Funcionarios/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _funcionarioService.RemoverAsync(id))
        {
            TempData["Erro"] = "Este funcionário tem registros no sistema e não pode ser excluído. Use \"Desligar\" para manter o histórico.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Sucesso"] = "Funcionário excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarAsync(Funcionario funcionario)
    {
        if (!string.IsNullOrWhiteSpace(funcionario.Cpf)
            && CpfAttribute.EhValido(funcionario.Cpf)
            && await _funcionarioService.ExisteCpfAsync(funcionario.Cpf, funcionario.Id == 0 ? null : funcionario.Id))
        {
            ModelState.AddModelError(nameof(Funcionario.Cpf), "Já existe um funcionário com este CPF.");
        }

        if (funcionario.DataAdmissao > DateTime.Today)
        {
            ModelState.AddModelError(nameof(Funcionario.DataAdmissao), "A data de admissão não pode ser no futuro.");
        }

        if (funcionario.DataDemissao.HasValue && funcionario.DataDemissao.Value < funcionario.DataAdmissao)
        {
            ModelState.AddModelError(nameof(Funcionario.DataDemissao), "O desligamento não pode ser antes da admissão.");
        }
    }

    private void LimparValidacaoDeNavegacao()
    {
        ModelState.Remove(nameof(Funcionario.MovimentacoesEstoqueRegistradas));
        ModelState.Remove(nameof(Funcionario.VacinacoesRegistradas));
        ModelState.Remove(nameof(Funcionario.RegistrosSaudeAtendidos));
    }
}
