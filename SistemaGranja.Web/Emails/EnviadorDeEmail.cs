using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace SistemaGranja.Web.Emails
{
    // Envia os e-mails do sistema (confirmação, esqueci a senha, convite).
    // Em desenvolvimento, sem servidor configurado, salva cada e-mail como arquivo .html para teste.
    public class EnviadorDeEmail : IEmailSender
    {
        private readonly ConfiguracaoEmail _config;
        private readonly ILogger<EnviadorDeEmail> _logger;

        public EnviadorDeEmail(IOptions<ConfiguracaoEmail> config, ILogger<EnviadorDeEmail> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (!_config.Configurado)
            {
                await SalvarParaTesteAsync(email, subject, htmlMessage);
                return;
            }

            using var mensagem = new MailMessage
            {
                From = new MailAddress(string.IsNullOrWhiteSpace(_config.Remetente) ? _config.Usuario : _config.Remetente, _config.NomeRemetente),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mensagem.To.Add(email);

            using var cliente = new SmtpClient(_config.Servidor, _config.Porta)
            {
                EnableSsl = _config.UsarSsl,
                Credentials = new NetworkCredential(_config.Usuario, _config.Senha)
            };

            await cliente.SendMailAsync(mensagem);
            _logger.LogInformation("E-mail \"{Assunto}\" enviado.", subject);
        }

        private async Task SalvarParaTesteAsync(string email, string assunto, string html)
        {
            if (string.IsNullOrWhiteSpace(_config.PastaDeTeste))
            {
                _logger.LogWarning("Servidor de e-mail não configurado. E-mail \"{Assunto}\" para {Email} não foi enviado.", assunto, email);
                return;
            }

            Directory.CreateDirectory(_config.PastaDeTeste);
            var nome = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}_{NomeDeArquivo(email)}.html";
            var caminho = Path.Combine(_config.PastaDeTeste, nome);
            var cabecalho = $"<p style=\"font-family:sans-serif;color:#666\">Para: {WebUtility.HtmlEncode(email)}<br>Assunto: {WebUtility.HtmlEncode(assunto)}</p><hr>";
            await File.WriteAllTextAsync(caminho, cabecalho + html);

            _logger.LogWarning("E-mail de teste salvo (servidor de e-mail não configurado): {Caminho}", caminho);
        }

        private static string NomeDeArquivo(string texto)
        {
            var invalidos = Path.GetInvalidFileNameChars().Concat(new[] { '@', ' ' }).ToHashSet();
            return new string(texto.Select(c => invalidos.Contains(c) ? '_' : c).ToArray());
        }
    }
}
