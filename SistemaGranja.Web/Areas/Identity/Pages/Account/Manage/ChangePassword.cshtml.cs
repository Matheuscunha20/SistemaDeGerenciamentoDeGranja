using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account.Manage
{
    // Trocar a senha (pede a senha atual)
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public DadosTrocaSenha Input { get; set; } = new();

        public class DadosTrocaSenha
        {
            [Required(ErrorMessage = "Informe a senha atual.")]
            [DataType(DataType.Password)]
            [Display(Name = "Senha atual")]
            public string SenhaAtual { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a nova senha.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Nova senha")]
            public string NovaSenha { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar nova senha")]
            [Compare(nameof(NovaSenha), ErrorMessage = "As senhas não conferem.")]
            public string ConfirmarSenha { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            var resultado = await _userManager.ChangePasswordAsync(usuario, Input.SenhaAtual, Input.NovaSenha);
            if (!resultado.Succeeded)
            {
                foreach (var erro in resultado.Errors)
                {
                    // "Senha incorreta" refere-se à senha atual
                    var campo = erro.Code == nameof(IdentityErrorDescriber.PasswordMismatch)
                        ? $"{nameof(Input)}.{nameof(Input.SenhaAtual)}"
                        : string.Empty;
                    ModelState.AddModelError(campo, erro.Code == nameof(IdentityErrorDescriber.PasswordMismatch) ? "Senha atual incorreta." : erro.Description);
                }
                return Page();
            }

            // A troca de senha encerra as sessões abertas em outros aparelhos; esta continua conectada
            await _signInManager.RefreshSignInAsync(usuario);
            _logger.LogInformation("Usuário trocou a senha.");
            TempData["Sucesso"] = "Senha alterada com sucesso.";
            return RedirectToPage();
        }
    }
}
