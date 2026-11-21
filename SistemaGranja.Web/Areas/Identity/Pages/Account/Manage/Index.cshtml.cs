using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using SistemaGranja.Web.Identidade;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account.Manage
{
    // Minha conta: dados básicos (telefone) e resumo da conta
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Email { get; private set; } = string.Empty;
        public bool EmailConfirmado { get; private set; }
        public string NomeFazenda { get; private set; } = string.Empty;
        public PerfilUsuario Perfil { get; private set; }

        [BindProperty]
        public DadosConta Input { get; set; } = new();

        public class DadosConta
        {
            [Phone(ErrorMessage = "Telefone inválido.")]
            [StringLength(20, ErrorMessage = "Use no máximo {1} caracteres.")]
            [Display(Name = "Telefone / WhatsApp")]
            public string? Telefone { get; set; }
        }

        private async Task CarregarAsync(ApplicationUser usuario)
        {
            Email = usuario.Email ?? string.Empty;
            EmailConfirmado = usuario.EmailConfirmed;
            Perfil = usuario.Perfil;
            NomeFazenda = User.FindFirst(FazendaDoUsuarioLogado.ClaimFazendaNome)?.Value ?? string.Empty;
            Input.Telefone ??= await _userManager.GetPhoneNumberAsync(usuario);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            await CarregarAsync(usuario);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            if (!ModelState.IsValid)
            {
                await CarregarAsync(usuario);
                return Page();
            }

            var telefone = string.IsNullOrWhiteSpace(Input.Telefone) ? null : Input.Telefone.Trim();
            if (telefone != await _userManager.GetPhoneNumberAsync(usuario))
            {
                var resultado = await _userManager.SetPhoneNumberAsync(usuario, telefone);
                if (!resultado.Succeeded)
                {
                    TempData["Erro"] = "Não foi possível salvar o telefone.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(usuario);
            TempData["Sucesso"] = "Seus dados foram salvos.";
            return RedirectToPage();
        }
    }
}
