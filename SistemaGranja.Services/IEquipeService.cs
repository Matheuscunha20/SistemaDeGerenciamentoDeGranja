using SistemaGranja.Data;
using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    // Equipe da fazenda: quem tem acesso, com qual perfil, e os convites
    public interface IEquipeService
    {
        Task<List<ApplicationUser>> ObterMembrosAsync();
        Task<List<Convite>> ObterConvitesPendentesAsync();

        // Retorna o convite criado ou uma mensagem de erro
        Task<(Convite? Convite, string? Erro)> CriarConviteAsync(string email, PerfilUsuario perfil);
        Task<bool> CancelarConviteAsync(int conviteId);

        // Convite válido (não aceito e não expirado) pelo código do link. Funciona sem login.
        Task<Convite?> ObterConviteValidoAsync(string? token);
        Task MarcarConviteAceitoAsync(Convite convite);

        // Ações do dono sobre outra pessoa da equipe. Retornam null se deu certo, ou a mensagem de erro.
        Task<string?> AlterarPerfilAsync(string usuarioId, PerfilUsuario novoPerfil, string usuarioLogadoId);
        Task<string?> RemoverAcessoAsync(string usuarioId, string usuarioLogadoId);
        Task<string?> ReativarAcessoAsync(string usuarioId);
    }
}
