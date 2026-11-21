using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Web.Emails;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account
{
    // Página aberta pelo link do e-mail de confirmação
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public bool Confirmado { get; private set; }

        public async Task OnGetAsync(string? userId, string? code)
        {
            var codigo = EmailsDaConta.DecodificarCodigo(code);
            if (string.IsNullOrEmpty(userId) || codigo == null) return;

            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null) return;

            var resultado = await _userManager.ConfirmEmailAsync(usuario, codigo);
            Confirmado = resultado.Succeeded;
        }
    }
}
