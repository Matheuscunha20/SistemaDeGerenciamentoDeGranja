using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IRacaService
    {
        // especieId opcional: filtra as raças de uma espécie
        Task<IEnumerable<Raca>> ObterTodasAsync(int? especieId = null);
        Task<Raca?> ObterPorIdAsync(int id);
        Task<bool> ExisteNomeAsync(int especieId, string nome, int? ignorarId = null);
        Task<int> ContarAnimaisAsync(int racaId);
        Task AdicionarAsync(Raca raca);
        Task AtualizarAsync(Raca raca);

        // Retorna false se a raça ainda tiver animais cadastrados
        Task<bool> RemoverAsync(int id);
    }
}
