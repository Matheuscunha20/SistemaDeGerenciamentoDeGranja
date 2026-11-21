using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Domain;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account
{
    // Substitui a tela de cadastro padrão (em inglês) do ASP.NET Core Identity
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly AppDbContext _db;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            AppDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _db = db;
        }

        [BindProperty]
        public DadosCadastro Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public class DadosCadastro
        {
            [Required(ErrorMessage = "Informe o nome da fazenda.")]
            [StringLength(150, ErrorMessage = "Use no máximo {1} caracteres.")]
            [Display(Name = "Nome da fazenda ou propriedade")]
            public string NomeFazenda { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe o e-mail.")]
            [EmailAddress(ErrorMessage = "E-mail inválido.")]
            [Display(Name = "E-mail")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a senha.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Senha { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
            public string ConfirmarSenha { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            // Fazenda, usuário e dados iniciais são criados juntos: ou tudo dá certo, ou nada fica gravado
            await using var transacao = await _db.Database.BeginTransactionAsync();

            var fazenda = new Fazenda { Nome = Input.NomeFazenda.Trim() };
            _db.Fazendas.Add(fazenda);
            await _db.SaveChangesAsync();

            var usuario = new ApplicationUser { UserName = Input.Email, Email = Input.Email, FazendaId = fazenda.Id };
            var resultado = await _userManager.CreateAsync(usuario, Input.Senha);

            if (!resultado.Succeeded)
            {
                await transacao.RollbackAsync();

                // Erros do Identity (senha fraca, e-mail já usado...) já vêm em português
                foreach (var erro in resultado.Errors)
                    ModelState.AddModelError(string.Empty, erro.Description);

                return Page();
            }

            // A fazenda nova já começa com espécies, raças e vacinas comuns
            _db.DefinirFazenda(fazenda.Id);
            await SemeadorDeDados.SemearFazendaNovaAsync(_db);

            await transacao.CommitAsync();
            _logger.LogInformation("Nova conta e fazenda criadas.");

            await _signInManager.SignInAsync(usuario, isPersistent: false);
            return LocalRedirect(ReturnUrl);
        }
    }
}
