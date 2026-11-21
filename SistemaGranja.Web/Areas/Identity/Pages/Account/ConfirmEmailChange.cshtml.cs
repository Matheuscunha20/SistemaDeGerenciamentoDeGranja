using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Web.Emails;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account
{
    // Página aberta pelo link enviado ao NOVO e-mail, ao trocar o e-mail da conta
    [AllowAnonymous]
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ConfirmEmailChangeModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public bool Confirmado { get; private set; }
        public string Mensagem { get; private set; } = "O link é inválido ou já expirou. Peça a troca de e-mail de novo em Minha conta.";

        public async Task OnGetAsync(string? userId, string? email, string? code)
        {
            var codigo = EmailsDaConta.DecodificarCodigo(code);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || codigo == null) return;

            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null) return;

            var resultado = await _userManager.ChangeEmailAsync(usuario, email, codigo);
            if (!resultado.Succeeded)
            {
                Mensagem = resultado.Errors.FirstOrDefault()?.Description ?? Mensagem;
                return;
            }

            // No AgroArca o e-mail também é o nome de usuário usado no login
            var nome = await _userManager.SetUserNameAsync(usuario, email);
            if (!nome.Succeeded)
            {
                Mensagem = nome.Errors.FirstOrDefault()?.Description ?? Mensagem;
                return;
            }

            Confirmado = true;
            if (_signInManager.IsSignedIn(User) && _userManager.GetUserId(User) == usuario.Id)
                await _signInManager.RefreshSignInAsync(usuario);
        }
    }
}
