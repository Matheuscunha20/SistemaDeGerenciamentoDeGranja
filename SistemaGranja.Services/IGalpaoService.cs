// Arquivo: IGalpaoService.cs (no projeto SistemaGranja.Services)
using SistemaGranja.Domain; // Para usar a classe Galpao
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IGalpaoService
    {
        Task<IEnumerable<Galpao>> ObterTodosAsync();
        Task<Galpao?> ObterPorIdAsync(int id); // Pode retornar null se não encontrar
        Task AdicionarAsync(Galpao galpao);
        Task AtualizarAsync(Galpao galpao);
        Task RemoverAsync(int id);
    }
}