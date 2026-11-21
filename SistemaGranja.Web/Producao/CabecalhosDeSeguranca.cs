namespace SistemaGranja.Web.Producao
{
    // Cabeçalhos que protegem contra ataques comuns do navegador
    public static class CabecalhosDeSeguranca
    {
        public static readonly IReadOnlyDictionary<string, string> Valores = new Dictionary<string, string>
        {
            // Não deixa o navegador "adivinhar" o tipo de arquivo
            ["X-Content-Type-Options"] = "nosniff",
            // Não deixa outro site abrir o AgroArca dentro de um quadro (clickjacking)
            ["X-Frame-Options"] = "DENY",
            // Ao clicar num link externo, envia só o endereço do site, sem o caminho
            ["Referrer-Policy"] = "strict-origin-when-cross-origin",
            // Recursos do aparelho que o sistema não usa ficam bloqueados
            ["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()",
            // Isola a janela de outras origens
            ["Cross-Origin-Opener-Policy"] = "same-origin"
        };

        public static void Aplicar(IHeaderDictionary cabecalhos)
        {
            foreach (var (nome, valor) in Valores)
                cabecalhos[nome] = valor;
        }

        public static IApplicationBuilder UseCabecalhosDeSeguranca(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    Aplicar(context.Response.Headers);
                    return Task.CompletedTask;
                });
                await next();
            });
        }
    }
}
