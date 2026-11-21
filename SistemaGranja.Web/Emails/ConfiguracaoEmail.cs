namespace SistemaGranja.Web.Emails
{
    // Dados do servidor de e-mail (seção "Email" da configuração / user-secrets).
    // Sem Servidor preenchido, os e-mails são salvos na PastaDeTeste em vez de enviados.
    public class ConfiguracaoEmail
    {
        public string Servidor { get; set; } = string.Empty;   // ex.: smtp.gmail.com
        public int Porta { get; set; } = 587;
        public bool UsarSsl { get; set; } = true;
        public string Usuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Remetente { get; set; } = string.Empty;  // e-mail que aparece como "De"
        public string NomeRemetente { get; set; } = "AgroArca";
        public string PastaDeTeste { get; set; } = string.Empty;

        public bool Configurado => !string.IsNullOrWhiteSpace(Servidor);
    }
}
