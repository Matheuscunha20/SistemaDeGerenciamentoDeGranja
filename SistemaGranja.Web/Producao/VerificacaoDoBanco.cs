using Microsoft.Extensions.Diagnostics.HealthChecks;
using SistemaGranja.Data;

namespace SistemaGranja.Web.Producao
{
    // Usada pelo endereço /saude: confere se o sistema consegue falar com o banco
    public class VerificacaoDoBanco : IHealthCheck
    {
        private readonly AppDbContext _db;

        public VerificacaoDoBanco(AppDbContext db)
        {
            _db = db;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _db.Database.CanConnectAsync(cancellationToken)
                    ? HealthCheckResult.Healthy("Banco de dados respondendo.")
                    : HealthCheckResult.Unhealthy("Sem conexão com o banco de dados.");
            }
            catch (Exception erro)
            {
                return HealthCheckResult.Unhealthy("Erro ao conectar no banco de dados.", erro);
            }
        }
    }
}
