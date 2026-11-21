namespace SistemaGranja.Web.Identidade
{
    // Páginas de conta que o AgroArca usa (todas em português).
    // As demais páginas prontas do ASP.NET Core Identity (em inglês, como autenticação em dois fatores
    // e login externo) ficam fora do ar até serem traduzidas e liberadas.
    public static class PaginasDaConta
    {
        private static readonly HashSet<string> Permitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "Login", "Logout", "Register", "AccessDenied", "AceitarConvite",
            "ForgotPassword", "ForgotPasswordConfirmation", "ResetPassword", "ResetPasswordConfirmation",
            "ConfirmEmail", "ConfirmEmailChange", "ResendEmailConfirmation",
            "Manage", "Manage/Index", "Manage/Email", "Manage/ChangePassword"
        };

        private const string Prefixo = "/Identity/Account";

        public static bool EstaLiberada(string? caminho)
        {
            if (string.IsNullOrEmpty(caminho) || !caminho.StartsWith(Prefixo, StringComparison.OrdinalIgnoreCase))
                return true; // não é página de conta

            var pagina = caminho.Substring(Prefixo.Length).Trim('/');
            return Permitidas.Contains(pagina);
        }
    }
}
