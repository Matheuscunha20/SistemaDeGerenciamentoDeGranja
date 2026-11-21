using System.Threading.RateLimiting;

namespace SistemaGranja.Web.Producao
{
    // Limita tentativas nas telas sensíveis (login, cadastro, esqueci a senha, convite),
    // para dificultar robôs que testam senhas ou criam contas em massa.
    public static class LimiteDeTentativas
    {
        public const int TentativasPorMinuto = 10;

        private static readonly string[] CaminhosProtegidos =
        {
            "/Identity/Account/Login",
            "/Identity/Account/Register",
            "/Identity/Account/ForgotPassword",
            "/Identity/Account/ResetPassword",
            "/Identity/Account/ResendEmailConfirmation",
            "/Identity/Account/AceitarConvite"
        };

        public static bool EhProtegido(HttpRequest pedido) =>
            HttpMethods.IsPost(pedido.Method)
            && CaminhosProtegidos.Any(c => pedido.Path.Equals(c, StringComparison.OrdinalIgnoreCase));

        public static void Configurar(Microsoft.AspNetCore.RateLimiting.RateLimiterOptions opcoes)
        {
            opcoes.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
            {
                if (!EhProtegido(contexto.Request))
                    return RateLimitPartition.GetNoLimiter("livre");

                // Uma "cota" por endereço de internet (IP)
                var ip = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = TentativasPorMinuto,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });

            opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opcoes.OnRejected = async (contexto, cancelamento) =>
            {
                contexto.HttpContext.Response.ContentType = "text/html; charset=utf-8";
                await contexto.HttpContext.Response.WriteAsync(
                    "<!DOCTYPE html><html lang=\"pt-BR\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>Muitas tentativas - AgroArca</title></head>" +
                    "<body style=\"font-family:system-ui,sans-serif;text-align:center;padding:48px 16px\"><h1>Muitas tentativas</h1>" +
                    "<p>Por segurança, aguarde um minuto e tente de novo.</p><p><a href=\"/\">Voltar ao início</a></p></body></html>",
                    cancelamento);
            };
        }
    }
}
