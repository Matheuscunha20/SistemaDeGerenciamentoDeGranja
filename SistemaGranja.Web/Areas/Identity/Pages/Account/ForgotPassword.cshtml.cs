using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Web.Emails;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account
{
    // "Esqueci minha senha": envia um link para criar uma nova senha
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailsDaConta _emails;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, EmailsDaConta emails)
        {
            _userManager = userManager;
            _emails = emails;
        }

        [BindProperty]
        public DadosEsqueciSenha Input { get; set; } = new();

        public class DadosEsqueciSenha
        {
            [Required(ErrorMessage = "Informe o e-mail.")]
            [EmailAddress(ErrorMessage = "E-mail inválido.")]
            [Display(Name = "E-mail")]
            public string Email { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var usuario = await _userManager.FindByEmailAsync(Input.Email);

            // Mesmo se o e-mail não existir, a resposta é igual: não revela quem tem conta
            if (usuario != null)
                await _emails.EnviarRedefinicaoDeSenhaAsync(usuario, Url, Request.Scheme);

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
