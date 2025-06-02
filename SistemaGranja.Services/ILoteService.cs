// Arquivo: ILoteService.cs
using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface ILoteService
    {
        Task<IEnumerable<Lote>> ObterTodosAsync();
        Task<Lote?> ObterPorIdAsync(int id);
        // Adicione AdicionarAsync, AtualizarAsync, RemoverAsync se precisar de CRUD completo para Lotes agora
    }
}