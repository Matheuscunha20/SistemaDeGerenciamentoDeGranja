using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account
{
    // Substitui a tela de cadastro padrão (em inglês) do ASP.NET Core Identity
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public DadosCadastro Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public class DadosCadastro
        {
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

            var usuario = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
            var resultado = await _userManager.CreateAsync(usuario, Input.Senha);

            if (resultado.Succeeded)
            {
                _logger.LogInformation("Nova conta criada.");
                await _signInManager.SignInAsync(usuario, isPersistent: false);
                return LocalRedirect(ReturnUrl);
            }

            // Erros do Identity (senha fraca, e-mail já usado...) já vêm em português
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError(string.Empty, erro.Description);

            return Page();
        }
    }
}
