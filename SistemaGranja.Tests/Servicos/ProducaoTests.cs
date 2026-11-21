using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SistemaGranja.Web.Producao;

namespace SistemaGranja.Tests.Servicos
{
    public class ProducaoTests
    {
        [Fact]
        public async Task Saude_informa_banco_respondendo()
        {
            using var db = BancoDeTeste.Criar();
            var resultado = await new VerificacaoDoBanco(db).CheckHealthAsync(new HealthCheckContext());
            Assert.Equal(HealthStatus.Healthy, resultado.Status);
        }

        [Fact]
        public void Cabecalhos_de_seguranca_sao_aplicados()
        {
            var contexto = new DefaultHttpContext();
            CabecalhosDeSeguranca.Aplicar(contexto.Response.Headers);

            Assert.Equal("nosniff", contexto.Response.Headers["X-Content-Type-Options"]);
            Assert.Equal("DENY", contexto.Response.Headers["X-Frame-Options"]);
            Assert.False(string.IsNullOrEmpty(contexto.Response.Headers["Referrer-Policy"]));
        }

        [Theory]
        [InlineData("POST", "/Identity/Account/Login", true)]
        [InlineData("POST", "/identity/account/register", true)]
        [InlineData("POST", "/Identity/Account/ForgotPassword", true)]
        [InlineData("GET", "/Identity/Account/Login", false)]
        [InlineData("POST", "/Animais/Create", false)]
        public void Limite_de_tentativas_so_nas_telas_sensiveis(string metodo, string caminho, bool protegido)
        {
            var contexto = new DefaultHttpContext();
            contexto.Request.Method = metodo;
            contexto.Request.Path = caminho;

            Assert.Equal(protegido, LimiteDeTentativas.EhProtegido(contexto.Request));
        }
    }
}
