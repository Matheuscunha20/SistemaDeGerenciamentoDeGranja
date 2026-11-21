using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGranja.Data;
using SistemaGranja.Services;
using SistemaGranja.Web.Identidade;

// Dados da fazenda (nome). Só o dono altera.
[Authorize(Policy = Politicas.Dono)]
public class FazendaController : Controller
{
    private readonly IFazendaService _fazendaService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public FazendaController(IFazendaService fazendaService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _fazendaService = fazendaService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public class DadosFazenda
    {
        [Display(Name = "Nome da fazenda ou propriedade")]
        [Required(ErrorMessage = "Informe o nome da fazenda.")]
        [StringLength(150, ErrorMessage = "Use no máximo {1} caracteres.")]
        public string Nome { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; }
    }

    // GET: Fazenda
    public async Task<IActionResult> Index()
    {
        var fazenda = await _fazendaService.ObterAtualAsync();
        if (fazenda == null) return NotFound();
        return View(new DadosFazenda { Nome = fazenda.Nome, DataCriacao = fazenda.DataCriacao });
    }

    // POST: Fazenda
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DadosFazenda dados)
    {
        if (ModelState.IsValid)
        {
            var erro = await _fazendaService.RenomearAsync(dados.Nome);
            if (erro == null)
            {
                // Atualiza o nome mostrado no menu sem precisar sair e entrar
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null) await _signInManager.RefreshSignInAsync(usuario);

                TempData["Sucesso"] = "Dados da fazenda salvos.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(nameof(dados.Nome), erro);
        }

        var fazenda = await _fazendaService.ObterAtualAsync();
        dados.DataCriacao = fazenda?.DataCriacao ?? dados.DataCriacao;
        return View(dados);
    }
}
