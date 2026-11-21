using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGranja.Domain;
using SistemaGranja.Web.Identidade;

namespace SistemaGranja.Tests.Servicos
{
    // Confere quem pode fazer o quê em cada tela
    public class PermissoesTests
    {
        private static ClaimsPrincipal Usuario(PerfilUsuario perfil) =>
            new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, perfil.ToString()) }, "teste"));

        [Theory]
        [InlineData(PerfilUsuario.Dono, true, true)]
        [InlineData(PerfilUsuario.Gerente, false, true)]
        [InlineData(PerfilUsuario.Funcionario, false, false)]
        public void Atalhos_de_permissao(PerfilUsuario perfil, bool ehDono, bool podeGerenciar)
        {
            var usuario = Usuario(perfil);
            Assert.Equal(ehDono, usuario.EhDono());
            Assert.Equal(podeGerenciar, usuario.PodeGerenciar());
            Assert.Equal(perfil.NomeExibicao(), usuario.NomeDoPerfil());
        }

        private static string? Politica(MemberInfo membro) =>
            membro.GetCustomAttributes<AuthorizeAttribute>().Select(a => a.Policy).FirstOrDefault(p => p != null);

        private static IEnumerable<Type> Controllers() =>
            typeof(EquipeController).Assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract);

        [Fact]
        public void Equipe_e_dados_da_fazenda_sao_so_do_dono()
        {
            Assert.Equal(Politicas.Dono, Politica(typeof(EquipeController)));
            Assert.Equal(Politicas.Dono, Politica(typeof(FazendaController)));
        }

        [Fact]
        public void Funcionario_nao_exclui_nada()
        {
            var semRestricao = new List<string>();
            foreach (var controller in Controllers())
            {
                if (Politica(controller) != null) continue;
                foreach (var acao in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                             .Where(m => m.Name.StartsWith("Delete")))
                {
                    if (Politica(acao) == null) semRestricao.Add($"{controller.Name}.{acao.Name}");
                }
            }
            Assert.True(semRestricao.Count == 0, "Exclusão liberada para funcionário: " + string.Join(", ", semRestricao));
        }

        [Theory]
        [InlineData("EspeciesController")]
        [InlineData("RacasController")]
        [InlineData("TiposVacinaController")]
        [InlineData("TiposAlimentoController")]
        [InlineData("FuncionariosController")]
        public void Cadastros_base_sao_de_dono_e_gerente(string nomeDoController)
        {
            var controller = Controllers().Single(t => t.Name == nomeDoController);
            Assert.Equal(Politicas.Gestao, Politica(controller));
        }
    }
}
