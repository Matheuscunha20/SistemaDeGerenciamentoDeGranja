using System.Net;

namespace SistemaGranja.Web.Emails
{
    // Textos dos e-mails do AgroArca, num visual simples que funciona em qualquer leitor de e-mail
    public static class ModelosDeEmail
    {
        public static (string Assunto, string Html) ConfirmarEmail(string link) => (
            "Confirme seu e-mail no AgroArca",
            Montar("Confirme seu e-mail",
                "Falta pouco! Clique no botão abaixo para confirmar o seu e-mail no AgroArca.",
                "Confirmar e-mail", link));

        public static (string Assunto, string Html) TrocarEmail(string link) => (
            "Confirme o seu novo e-mail no AgroArca",
            Montar("Confirme o novo e-mail",
                "Você pediu para trocar o e-mail da sua conta no AgroArca. Clique no botão para confirmar. Se não foi você, ignore esta mensagem.",
                "Confirmar novo e-mail", link));

        public static (string Assunto, string Html) RedefinirSenha(string link) => (
            "Redefinir sua senha do AgroArca",
            Montar("Redefinir senha",
                "Recebemos um pedido para redefinir a sua senha. Clique no botão para criar uma nova. Se não foi você, ignore esta mensagem: sua senha continua a mesma.",
                "Criar nova senha", link));

        public static (string Assunto, string Html) Convite(string nomeFazenda, string perfil, string link, int diasDeValidade) => (
            $"Convite para a equipe da {nomeFazenda} no AgroArca",
            Montar("Você foi convidado!",
                $"Você foi convidado(a) para a equipe da {nomeFazenda} no AgroArca, com o perfil {perfil}. Clique no botão para criar sua senha. O convite vale por {diasDeValidade} dias.",
                "Aceitar convite", link));

        // Monta o HTML do e-mail. Todo texto é escapado (o nome da fazenda vem do usuário).
        public static string Montar(string titulo, string texto, string textoDoBotao, string link)
        {
            var t = WebUtility.HtmlEncode(titulo);
            var p = WebUtility.HtmlEncode(texto);
            var b = WebUtility.HtmlEncode(textoDoBotao);
            var l = WebUtility.HtmlEncode(link);
            return $@"<div style=""font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;padding:24px;color:#1f2a1f"">
  <div style=""font-size:22px;font-weight:bold;color:#2e7d32;margin-bottom:16px"">AgroArca</div>
  <h1 style=""font-size:20px;margin:0 0 12px"">{t}</h1>
  <p style=""font-size:15px;line-height:1.5;margin:0 0 24px"">{p}</p>
  <p style=""margin:0 0 24px""><a href=""{l}"" style=""background:#2e7d32;color:#fff;text-decoration:none;padding:12px 22px;border-radius:6px;display:inline-block;font-weight:bold"">{b}</a></p>
  <p style=""font-size:12px;color:#666;line-height:1.5"">Se o botão não funcionar, copie e cole este endereço no navegador:<br><span style=""word-break:break-all"">{l}</span></p>
  <hr style=""border:none;border-top:1px solid #ddd;margin:24px 0"">
  <p style=""font-size:12px;color:#888"">AgroArca · Cuide do seu rebanho como um bom pastor.</p>
</div>";
        }
    }
}
