using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IGalpaoService
    {
        Task<IEnumerable<Galpao>> ObterTodosAsync();
        Task<Galpao?> ObterPorIdAsync(int id);

        // Galpão com os animais que estão nele (para a tela de detalhes)
        Task<Galpao?> ObterComAnimaisAsync(int id);

        // Quantidade de animais em cada galpão: chave = Id do galpão
        Task<Dictionary<int, int>> ObterOcupacaoAsync();

        // Quantidade de animais num galpão, podendo ignorar um animal (útil ao editar)
        Task<int> ContarAnimaisAsync(int galpaoId, int? ignorarAnimalId = null);

        Task<bool> ExisteNomeAsync(string nome, int? ignorarId = null);
        Task AdicionarAsync(Galpao galpao);
        Task AtualizarAsync(Galpao galpao);

        // Retorna false se ainda houver animais no galpão
        Task<bool> RemoverAsync(int id);
    }
}
