using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SistemaGranja.Web.Emails;
using SistemaGranja.Web.Identidade;

namespace SistemaGranja.Tests.Servicos
{
    public class EmailsTests
    {
        [Fact]
        public void Modelo_escapa_texto_vindo_do_usuario()
        {
            var (assunto, html) = ModelosDeEmail.Convite("<script>alert(1)</script>", "Gerente", "https://agroarca.test/convite?token=abc&x=1", 7);

            Assert.Contains("Convite", assunto);
            Assert.DoesNotContain("<script>", html);
            Assert.Contains("&lt;script&gt;", html);
            Assert.Contains("token=abc&amp;x=1", html);
        }

        [Fact]
        public void Codigo_do_link_vai_e_volta_sem_perder_nada()
        {
            var original = "CfDJ8+abc/123==";
            var codificado = EmailsDaConta.CodificarCodigo(original);

            Assert.DoesNotContain("+", codificado);
            Assert.DoesNotContain("/", codificado);
            Assert.Equal(original, EmailsDaConta.DecodificarCodigo(codificado));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("@@@ nao e base64 @@@")]
        public void Codigo_invalido_vira_null(string? codigo)
        {
            Assert.Null(EmailsDaConta.DecodificarCodigo(codigo));
        }

        [Fact]
        public async Task Sem_servidor_configurado_salva_o_email_como_arquivo()
        {
            var pasta = Path.Combine(Path.GetTempPath(), "agroarca-emails-" + Guid.NewGuid());
            try
            {
                var enviador = new EnviadorDeEmail(
                    Options.Create(new ConfiguracaoEmail { PastaDeTeste = pasta }),
                    NullLogger<EnviadorDeEmail>.Instance);

                await enviador.SendEmailAsync("joao@fazenda.com", "Teste", "<p>Olá</p>");

                var arquivo = Assert.Single(Directory.GetFiles(pasta, "*.html"));
                var conteudo = await File.ReadAllTextAsync(arquivo);
                Assert.Contains("joao@fazenda.com", conteudo);
                Assert.Contains("<p>Olá</p>", conteudo);
            }
            finally
            {
                if (Directory.Exists(pasta)) Directory.Delete(pasta, true);
            }
        }

        [Theory]
        [InlineData("/Identity/Account/Login", true)]
        [InlineData("/Identity/Account/ForgotPassword", true)]
        [InlineData("/Identity/Account/Manage", true)]
        [InlineData("/Identity/Account/Manage/ChangePassword", true)]
        [InlineData("/identity/account/manage/email", true)]
        [InlineData("/Identity/Account/Manage/TwoFactorAuthentication", false)]
        [InlineData("/Identity/Account/Manage/PersonalData", false)]
        [InlineData("/Identity/Account/ExternalLogin", false)]
        [InlineData("/Animais", true)]
        [InlineData("/", true)]
        public void So_paginas_de_conta_em_portugues_ficam_no_ar(string caminho, bool liberada)
        {
            Assert.Equal(liberada, PaginasDaConta.EstaLiberada(caminho));
        }
    }
}
