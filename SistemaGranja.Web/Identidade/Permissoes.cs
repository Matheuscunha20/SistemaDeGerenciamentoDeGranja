using System.Security.Claims;
using SistemaGranja.Domain;

namespace SistemaGranja.Web.Identidade
{
    // Nomes das regras de acesso usadas em [Authorize(Policy = ...)]
    public static class Politicas
    {
        // Dono e Gerente: cadastros base, exclusões, galpões e lotes
        public const string Gestao = "Gestao";

        // Só o Dono: equipe e dados da fazenda
        public const string Dono = "Dono";
    }

    // Atalhos para as telas saberem o que mostrar
    public static class PermissoesDoUsuario
    {
        public static bool EhDono(this ClaimsPrincipal usuario) =>
            usuario.IsInRole(nameof(PerfilUsuario.Dono));

        // Dono ou Gerente
        public static bool PodeGerenciar(this ClaimsPrincipal usuario) =>
            usuario.IsInRole(nameof(PerfilUsuario.Dono)) || usuario.IsInRole(nameof(PerfilUsuario.Gerente));

        public static string NomeDoPerfil(this ClaimsPrincipal usuario)
        {
            foreach (var perfil in Enum.GetValues<PerfilUsuario>())
                if (usuario.IsInRole(perfil.ToString())) return perfil.NomeExibicao();
            return string.Empty;
        }
    }
}
