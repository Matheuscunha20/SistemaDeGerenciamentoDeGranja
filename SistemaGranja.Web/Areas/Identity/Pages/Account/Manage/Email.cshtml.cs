using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Web.Emails;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account.Manage
{
    // Trocar o e-mail (precisa confirmar pelo link enviado ao novo endereço) e reenviar a confirmação
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailsDaConta _emails;

        public EmailModel(UserManager<ApplicationUser> userManager, EmailsDaConta emails)
        {
            _userManager = userManager;
            _emails = emails;
        }

        public string EmailAtual { get; private set; } = string.Empty;
        public bool EmailConfirmado { get; private set; }

        [BindProperty]
        public DadosNovoEmail Input { get; set; } = new();

        public class DadosNovoEmail
        {
            [Required(ErrorMessage = "Informe o novo e-mail.")]
            [EmailAddress(ErrorMessage = "E-mail inválido.")]
            [Display(Name = "Novo e-mail")]
            public string NovoEmail { get; set; } = string.Empty;
        }

        private void Carregar(ApplicationUser usuario)
        {
            EmailAtual = usuario.Email ?? string.Empty;
            EmailConfirmado = usuario.EmailConfirmed;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            Carregar(usuario);
            return Page();
        }

        public async Task<IActionResult> OnPostTrocarAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();
            Carregar(usuario);

            if (!ModelState.IsValid)
                return Page();

            var novoEmail = Input.NovoEmail.Trim();
            if (string.Equals(novoEmail, usuario.Email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(Input) + "." + nameof(Input.NovoEmail), "Este já é o seu e-mail atual.");
                return Page();
            }

            if (await _userManager.FindByEmailAsync(novoEmail) != null)
            {
                ModelState.AddModelError(nameof(Input) + "." + nameof(Input.NovoEmail), "Este e-mail já é usado por outra conta.");
                return Page();
            }

            if (await _emails.EnviarTrocaDeEmailAsync(usuario, novoEmail, Url, Request.Scheme))
                TempData["Sucesso"] = $"Enviamos um link para {novoEmail}. O e-mail só muda depois que você clicar no link.";
            else
                TempData["Erro"] = "Não foi possível enviar o e-mail agora. Tente de novo em alguns minutos.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReenviarAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            if (usuario.EmailConfirmed)
                TempData["Sucesso"] = "Seu e-mail já está confirmado.";
            else if (await _emails.EnviarConfirmacaoAsync(usuario, Url, Request.Scheme))
                TempData["Sucesso"] = $"Enviamos o link de confirmação para {usuario.Email}.";
            else
                TempData["Erro"] = "Não foi possível enviar o e-mail agora. Tente de novo em alguns minutos.";

            return RedirectToPage();
        }
    }
}
