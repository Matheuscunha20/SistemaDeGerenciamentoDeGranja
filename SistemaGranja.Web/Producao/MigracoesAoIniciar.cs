using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;

namespace SistemaGranja.Web.Producao
{
    // Atualiza o banco (migrations) quando o sistema inicia.
    // No servidor o banco pode demorar alguns segundos para ficar pronto, então tenta de novo antes de desistir.
    public static class MigracoesAoIniciar
    {
        public static async Task AplicarAsync(IServiceProvider servicos, ILogger logger, int tentativas = 20, int segundosEntreTentativas = 3)
        {
            for (var tentativa = 1; ; tentativa++)
            {
                try
                {
                    using var escopo = servicos.CreateScope();
                    var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
                    var pendentes = (await db.Database.GetPendingMigrationsAsync()).ToList();
                    if (pendentes.Count > 0)
                    {
                        logger.LogInformation("Aplicando {Quantidade} atualização(ões) no banco: {Nomes}", pendentes.Count, string.Join(", ", pendentes));
                        await db.Database.MigrateAsync();
                    }
                    logger.LogInformation("Banco de dados pronto.");
                    return;
                }
                catch (Exception erro) when (tentativa < tentativas)
                {
                    logger.LogWarning("Banco ainda não respondeu (tentativa {Tentativa} de {Total}): {Mensagem}", tentativa, tentativas, erro.Message);
                    await Task.Delay(TimeSpan.FromSeconds(segundosEntreTentativas));
                }
                catch (Exception erro)
                {
                    logger.LogCritical(erro,
                        "Não foi possível conectar no banco de dados. Confira se o MySQL está ligado " +
                        "(no computador: abra o Docker Desktop e rode \"docker compose up -d\") e se a conexão está certa.");
                    throw;
                }
            }
        }
    }
}
