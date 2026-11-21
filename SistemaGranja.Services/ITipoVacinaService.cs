using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface ITipoVacinaService
    {
        Task<IEnumerable<TipoVacina>> ObterTodosAsync();
        Task<TipoVacina?> ObterPorIdAsync(int id);
        Task<bool> ExisteNomeAsync(string nome, int? ignorarId = null);

        // Quantas aplicações existem por vacina: chave = Id da vacina
        Task<Dictionary<int, int>> ObterQuantidadeAplicacoesAsync();

        Task AdicionarAsync(TipoVacina tipoVacina);
        Task AtualizarAsync(TipoVacina tipoVacina);

        // Retorna false se a vacina já tiver sido aplicada (para não apagar o histórico)
        Task<bool> RemoverAsync(int id);
    }
}
