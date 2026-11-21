using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SistemaGranja.Data;

namespace SistemaGranja.Web.Emails
{
    // Gera os links seguros (com código) e envia os e-mails da conta.
    // Os métodos retornam false se o envio falhar, para a tela avisar sem quebrar.
    public class EmailsDaConta
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailsDaConta> _logger;

        public EmailsDaConta(UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<EmailsDaConta> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // O código vai na URL: precisa ser convertido para um formato seguro para links
        public static string CodificarCodigo(string codigo) => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codigo));

        public static string? DecodificarCodigo(string? codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;
            try { return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(codigo)); }
            catch (FormatException) { return null; }
        }

        public async Task<bool> EnviarConfirmacaoAsync(ApplicationUser usuario, IUrlHelper url, string esquema)
        {
            var codigo = CodificarCodigo(await _userManager.GenerateEmailConfirmationTokenAsync(usuario));
            var link = url.Page("/Account/ConfirmEmail", null, new { area = "Identity", userId = usuario.Id, code = codigo }, esquema)!;
            var (assunto, html) = ModelosDeEmail.ConfirmarEmail(link);
            return await EnviarAsync(usuario.Email!, assunto, html);
        }

        public async Task<bool> EnviarTrocaDeEmailAsync(ApplicationUser usuario, string novoEmail, IUrlHelper url, string esquema)
        {
            var codigo = CodificarCodigo(await _userManager.GenerateChangeEmailTokenAsync(usuario, novoEmail));
            var link = url.Page("/Account/ConfirmEmailChange", null, new { area = "Identity", userId = usuario.Id, email = novoEmail, code = codigo }, esquema)!;
            var (assunto, html) = ModelosDeEmail.TrocarEmail(link);
            return await EnviarAsync(novoEmail, assunto, html);
        }

        public async Task<bool> EnviarRedefinicaoDeSenhaAsync(ApplicationUser usuario, IUrlHelper url, string esquema)
        {
            var codigo = CodificarCodigo(await _userManager.GeneratePasswordResetTokenAsync(usuario));
            var link = url.Page("/Account/ResetPassword", null, new { area = "Identity", code = codigo }, esquema)!;
            var (assunto, html) = ModelosDeEmail.RedefinirSenha(link);
            return await EnviarAsync(usuario.Email!, assunto, html);
        }

        public async Task<bool> EnviarAsync(string para, string assunto, string html)
        {
            try
            {
                await _emailSender.SendEmailAsync(para, assunto, html);
                return true;
            }
            catch (Exception erro)
            {
                _logger.LogError(erro, "Falha ao enviar o e-mail \"{Assunto}\".", assunto);
                return false;
            }
        }
    }
}
