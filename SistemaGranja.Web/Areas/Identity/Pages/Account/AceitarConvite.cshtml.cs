using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using SistemaGranja.Services;

namespace SistemaGranja.Web.Areas.Identity.Pages.Account
{
    // Página aberta pelo link do convite: a pessoa cria a senha e entra na equipe da fazenda
    [AllowAnonymous]
    public class AceitarConviteModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEquipeService _equipeService;
        private readonly AppDbContext _db;
        private readonly ILogger<AceitarConviteModel> _logger;

        public AceitarConviteModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEquipeService equipeService,
            AppDbContext db,
            ILogger<AceitarConviteModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _equipeService = equipeService;
            _db = db;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string? Token { get; set; }

        [BindProperty]
        public DadosConvite Input { get; set; } = new();

        public Convite? Convite { get; private set; }
        public string NomeFazenda { get; private set; } = string.Empty;
        public bool JaTemConta { get; private set; }

        public class DadosConvite
        {
            [Required(ErrorMessage = "Informe a senha.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Crie uma senha")]
            public string Senha { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
            public string ConfirmarSenha { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            await CarregarConviteAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CarregarConviteAsync();
            if (Convite == null || JaTemConta || User.Identity?.IsAuthenticated == true)
                return Page();

            if (!ModelState.IsValid)
                return Page();

            // Usuário e convite aceito são gravados juntos
            await using var transacao = await _db.Database.BeginTransactionAsync();

            var usuario = new ApplicationUser
            {
                UserName = Convite.Email,
                Email = Convite.Email,
                FazendaId = Convite.FazendaId,
                Perfil = Convite.Perfil
            };
            var resultado = await _userManager.CreateAsync(usuario, Input.Senha);
            if (!resultado.Succeeded)
            {
                await transacao.RollbackAsync();
                foreach (var erro in resultado.Errors)
                    ModelState.AddModelError(string.Empty, erro.Description);
                return Page();
            }

            await _equipeService.MarcarConviteAceitoAsync(Convite);
            await transacao.CommitAsync();
            _logger.LogInformation("Convite aceito: nova pessoa na equipe da fazenda {FazendaId}.", Convite.FazendaId);

            await _signInManager.SignInAsync(usuario, isPersistent: false);
            TempData["Sucesso"] = $"Bem-vindo(a) à equipe da {NomeFazenda}!";
            return LocalRedirect(Url.Content("~/"));
        }

        private async Task CarregarConviteAsync()
        {
            Convite = await _equipeService.ObterConviteValidoAsync(Token);
            if (Convite == null) return;

            var fazenda = await _db.Fazendas.FindAsync(Convite.FazendaId);
            NomeFazenda = fazenda?.Nome ?? "fazenda";
            JaTemConta = await _userManager.FindByEmailAsync(Convite.Email) != null;
        }
    }
}
