using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Services;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account
{
    // Substitui a tela de login padrão (em inglês) do ASP.NET Core Identity
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public DadosLogin Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public class DadosLogin
        {
            [Required(ErrorMessage = "Informe o e-mail.")]
            [EmailAddress(ErrorMessage = "E-mail inválido.")]
            [Display(Name = "E-mail")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a senha.")]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Senha { get; set; } = string.Empty;

            [Display(Name = "Manter conectado")]
            public bool LembrarDeMim { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            // Limpa um login externo pela metade, se houver
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            // lockoutOnFailure: após várias senhas erradas, a conta fica bloqueada por alguns minutos
            var resultado = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Senha, Input.LembrarDeMim, lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                _logger.LogInformation("Usuário entrou no sistema.");
                return LocalRedirect(ReturnUrl);
            }

            if (resultado.IsLockedOut)
            {
                // O dono da fazenda removeu o acesso desta pessoa
                var usuario = await _userManager.FindByEmailAsync(Input.Email);
                if (usuario != null && EquipeService.AcessoRemovido(usuario))
                {
                    ModelState.AddModelError(string.Empty, "Seu acesso a esta fazenda foi removido. Fale com o dono da fazenda.");
                    return Page();
                }

                ModelState.AddModelError(string.Empty, "Conta bloqueada por excesso de tentativas. Tente novamente em alguns minutos.");
                return Page();
            }

            // Mensagem genérica: não revela se o e-mail existe
            ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
            return Page();
        }
    }
}
