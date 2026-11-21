using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IEspecieService
    {
        Task<IEnumerable<Especie>> ObterTodasAsync();
        Task<Especie?> ObterPorIdAsync(int id);
        Task<bool> ExisteNomeAsync(string nome, int? ignorarId = null);
        Task AdicionarAsync(Especie especie);
        Task AtualizarAsync(Especie especie);

        // Retorna false se a espécie ainda tiver raças cadastradas
        Task<bool> RemoverAsync(int id);
    }
}
