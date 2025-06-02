// Arquivo: ITipoAlimentoService.cs (no projeto SistemaGranja.Services)
using SistemaGranja.Domain; // Para usar a classe TipoAlimento
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface ITipoAlimentoService
    {
        Task<IEnumerable<TipoAlimento>> ObterTodosAsync();
        Task<TipoAlimento?> ObterPorIdAsync(int id);
        Task AdicionarAsync(TipoAlimento tipoAlimento);
        Task AtualizarAsync(TipoAlimento tipoAlimento);
        Task RemoverAsync(int id);
    }
}