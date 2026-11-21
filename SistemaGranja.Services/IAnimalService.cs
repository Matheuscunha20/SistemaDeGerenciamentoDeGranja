using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IAnimalService
    {
        Task<IEnumerable<Animal>> ObterTodosComRelacionamentosAsync();

        // Lista com filtros opcionais: texto (tag ou nome), espécie, galpão e estado de saúde
        Task<IEnumerable<Animal>> ObterFiltradosAsync(string? busca, int? especieId, int? galpaoId, string? statusSaude);

        Task<Animal?> ObterPorIdComRelacionamentosAsync(int id);
        Task<bool> ExisteTagAsync(string tag, int? ignorarId = null);
        Task AdicionarAsync(Animal animal);
        Task AtualizarAsync(Animal animal);
        Task RemoverAsync(int id);
    }
}
