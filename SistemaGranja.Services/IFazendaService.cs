using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    // Dados da fazenda do usuário logado
    public interface IFazendaService
    {
        Task<Fazenda?> ObterAtualAsync();
        Task<string?> RenomearAsync(string novoNome);
    }
}
