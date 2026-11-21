using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface ITipoAlimentoService
    {
        Task<IEnumerable<TipoAlimento>> ObterTodosAsync();
        Task<TipoAlimento?> ObterPorIdAsync(int id);
        Task<bool> ExisteNomeAsync(string nome, int? ignorarId = null);
        Task<int> ContarMovimentacoesAsync(int tipoAlimentoId);
        Task AdicionarAsync(TipoAlimento tipoAlimento);
        Task AtualizarAsync(TipoAlimento tipoAlimento);

        // Retorna false se o alimento tiver movimentações de estoque
        Task<bool> RemoverAsync(int id);
    }
}
