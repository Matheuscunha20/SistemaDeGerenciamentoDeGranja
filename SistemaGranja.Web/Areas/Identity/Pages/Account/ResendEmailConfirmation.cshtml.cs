using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Web.Emails;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account
{
    // Reenvia o e-mail de confirmação de cadastro
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailsDaConta _emails;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, EmailsDaConta emails)
        {
            _userManager = userManager;
            _emails = emails;
        }

        [BindProperty]
        public DadosReenvio Input { get; set; } = new();

        public bool Enviado { get; private set; }

        public class DadosReenvio
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
            if (usuario != null && !usuario.EmailConfirmed)
                await _emails.EnviarConfirmacaoAsync(usuario, Url, Request.Scheme);

            // Resposta igual sempre: não revela quem tem conta
            Enviado = true;
            return Page();
        }
    }
}
