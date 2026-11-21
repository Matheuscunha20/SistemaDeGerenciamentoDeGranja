using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Web.Emails;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account
{
    // Página aberta pelo link do e-mail "redefinir senha"
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public DadosNovaSenha Input { get; set; } = new();

        public bool LinkInvalido { get; private set; }

        public class DadosNovaSenha
        {
            [Required(ErrorMessage = "Informe o e-mail.")]
            [EmailAddress(ErrorMessage = "E-mail inválido.")]
            [Display(Name = "E-mail")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a nova senha.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Nova senha")]
            public string Senha { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar nova senha")]
            [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
            public string ConfirmarSenha { get; set; } = string.Empty;

            public string Codigo { get; set; } = string.Empty;
        }

        public void OnGet(string? code = null)
        {
            var codigo = EmailsDaConta.DecodificarCodigo(code);
            if (codigo == null)
            {
                LinkInvalido = true;
                return;
            }
            Input = new DadosNovaSenha { Codigo = code! };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var codigo = EmailsDaConta.DecodificarCodigo(Input.Codigo);
            if (codigo == null)
            {
                LinkInvalido = true;
                return Page();
            }

            if (!ModelState.IsValid)
                return Page();

            var usuario = await _userManager.FindByEmailAsync(Input.Email);
            if (usuario == null)
            {
                // Não revela se o e-mail existe
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var resultado = await _userManager.ResetPasswordAsync(usuario, codigo, Input.Senha);
            if (resultado.Succeeded)
            {
                // Quem pediu a nova senha também provou ter acesso ao e-mail
                if (!usuario.EmailConfirmed)
                {
                    usuario.EmailConfirmed = true;
                    await _userManager.UpdateAsync(usuario);
                }
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var erro in resultado.Errors)
                ModelState.AddModelError(string.Empty, erro.Description);
            return Page();
        }
    }
}
