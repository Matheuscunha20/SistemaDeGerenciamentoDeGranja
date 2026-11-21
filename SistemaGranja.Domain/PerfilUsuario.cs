using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SistemaGranja.Domain
{
    // Perfil de acesso de quem usa o sistema. Os números ficam gravados no banco: não altere os existentes.
    public enum PerfilUsuario
    {
        // Acesso total, inclusive equipe e dados da fazenda
        [Display(Name = "Dono")] Dono = 1,

        // Opera tudo (cadastros, exclusões), mas não mexe na equipe
        [Display(Name = "Gerente")] Gerente = 2,

        // Dia a dia: registra e edita, mas não exclui nem altera cadastros base
        [Display(Name = "Funcionário")] Funcionario = 3
    }

    public static class PerfilUsuarioExtensions
    {
        public static string NomeExibicao(this PerfilUsuario perfil)
        {
            var campo = typeof(PerfilUsuario).GetField(perfil.ToString());
            return campo?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? perfil.ToString();
        }

        // O que cada perfil pode fazer, para mostrar na tela da equipe
        public static string Descricao(this PerfilUsuario perfil) => perfil switch
        {
            PerfilUsuario.Dono => "Acesso total, inclusive equipe e dados da fazenda.",
            PerfilUsuario.Gerente => "Opera tudo, inclusive cadastros e exclusões, mas não mexe na equipe.",
            _ => "Registra o dia a dia (animais, vacinas, saúde, estoque). Não exclui e não altera cadastros."
        };

        public static string CorExibicao(this PerfilUsuario perfil) => perfil switch
        {
            PerfilUsuario.Dono => "bg-success",
            PerfilUsuario.Gerente => "bg-primary",
            _ => "bg-secondary"
        };
    }
}
