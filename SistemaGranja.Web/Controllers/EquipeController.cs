using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using SistemaGranja.Web.Identidade;
using SistemaGranja.Web.Emails;

// Equipe da fazenda: só o dono vê e altera
[Authorize(Policy = Politicas.Dono)]
public class EquipeController : Controller
{
    private readonly IEquipeService _equipeService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailsDaConta _emails;

    public EquipeController(IEquipeService equipeService, UserManager<ApplicationUser> userManager, EmailsDaConta emails)
    {
        _equipeService = equipeService;
        _userManager = userManager;
        _emails = emails;
    }

    public class NovoConvite
    {
        [Display(Name = "E-mail da pessoa")]
        [Required(ErrorMessage = "Informe o e-mail da pessoa.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Perfil")]
        public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Funcionario;
    }

    public class TelaEquipe
    {
        public List<ApplicationUser> Membros { get; set; } = new();
        public List<Convite> Convites { get; set; } = new();
        public string UsuarioLogadoId { get; set; } = string.Empty;
        public NovoConvite NovoConvite { get; set; } = new();
        // Convite recém-criado, para mostrar o link
        public int? ConviteCriadoId { get; set; }
    }

    // GET: Equipe
    public async Task<IActionResult> Index(int? conviteCriado)
    {
        return View(await MontarTelaAsync(new NovoConvite(), conviteCriado));
    }

    // POST: Equipe/Convidar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Convidar([Bind(Prefix = "NovoConvite")] NovoConvite novoConvite)
    {
        if (ModelState.IsValid)
        {
            var (convite, erro) = await _equipeService.CriarConviteAsync(novoConvite.Email, novoConvite.Perfil);
            if (convite != null)
            {
                // Além do link para copiar/WhatsApp, o convite também vai por e-mail
                var link = Url.Page("/Account/AceitarConvite", null, new { area = "Identity", token = convite.Token }, Request.Scheme)!;
                var nomeFazenda = User.FindFirst(FazendaDoUsuarioLogado.ClaimFazendaNome)?.Value ?? "fazenda";
                var (assunto, html) = ModelosDeEmail.Convite(nomeFazenda, convite.Perfil.NomeExibicao(), link, Convite.DiasDeValidade);

                TempData["Sucesso"] = await _emails.EnviarAsync(convite.Email, assunto, html)
                    ? $"Convite enviado por e-mail para {convite.Email}. Se preferir, copie o link abaixo e mande pelo WhatsApp."
                    : "Convite criado, mas não foi possível enviar o e-mail. Copie o link abaixo e envie para a pessoa.";
                return RedirectToAction(nameof(Index), new { conviteCriado = convite.Id });
            }
            ModelState.AddModelError("NovoConvite.Email", erro!);
        }

        return View(nameof(Index), await MontarTelaAsync(novoConvite, null));
    }

    // POST: Equipe/CancelarConvite/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarConvite(int id)
    {
        if (await _equipeService.CancelarConviteAsync(id))
            TempData["Sucesso"] = "Convite cancelado. O link deixou de funcionar.";
        else
            TempData["Erro"] = "Convite não encontrado.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Equipe/AlterarPerfil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarPerfil(string id, PerfilUsuario perfil)
    {
        var erro = await _equipeService.AlterarPerfilAsync(id, perfil, UsuarioLogadoId());
        if (erro == null) TempData["Sucesso"] = $"Perfil alterado para {perfil.NomeExibicao()}. Vale em até 1 minuto.";
        else TempData["Erro"] = erro;
        return RedirectToAction(nameof(Index));
    }

    // POST: Equipe/RemoverAcesso
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverAcesso(string id)
    {
        var erro = await _equipeService.RemoverAcessoAsync(id, UsuarioLogadoId());
        if (erro == null) TempData["Sucesso"] = "Acesso removido. A pessoa não consegue mais entrar nesta fazenda.";
        else TempData["Erro"] = erro;
        return RedirectToAction(nameof(Index));
    }

    // POST: Equipe/ReativarAcesso
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReativarAcesso(string id)
    {
        var erro = await _equipeService.ReativarAcessoAsync(id);
        if (erro == null) TempData["Sucesso"] = "Acesso reativado.";
        else TempData["Erro"] = erro;
        return RedirectToAction(nameof(Index));
    }

    private string UsuarioLogadoId() => _userManager.GetUserId(User) ?? string.Empty;

    private async Task<TelaEquipe> MontarTelaAsync(NovoConvite novoConvite, int? conviteCriado)
    {
        return new TelaEquipe
        {
            Membros = await _equipeService.ObterMembrosAsync(),
            Convites = await _equipeService.ObterConvitesPendentesAsync(),
            UsuarioLogadoId = UsuarioLogadoId(),
            NovoConvite = novoConvite,
            ConviteCriadoId = conviteCriado
        };
    }
}
