using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface ILoteService
    {
        // ativo: true = só ativos, false = só encerrados, null = todos
        Task<IEnumerable<Lote>> ObterTodosAsync(bool? ativo = null);

        // Lotes ativos, para a escolha de lote no cadastro de animais
        Task<IEnumerable<Lote>> ObterAtivosAsync();

        Task<Lote?> ObterPorIdAsync(int id);

        // Lote com os animais (para a tela de detalhes)
        Task<Lote?> ObterComAnimaisAsync(int id);

        // Animais do lote que já saíram do rebanho (vendidos, mortos...)
        Task<List<Animal>> ObterAnimaisQueSairamAsync(int loteId);

        // Quantidade de animais em cada lote: chave = Id do lote
        Task<Dictionary<int, int>> ObterQuantidadeAnimaisAsync();

        // Animais que ainda não estão neste lote, com filtros opcionais
        Task<IEnumerable<Animal>> ObterAnimaisForaDoLoteAsync(int loteId, string? busca, int? especieId, int? galpaoId);

        Task<bool> ExisteCodigoAsync(string codigo, int? ignorarId = null);
        Task AdicionarAsync(Lote lote);
        Task AtualizarAsync(Lote lote);
        Task AlterarSituacaoAsync(int id, bool ativo);

        // Coloca vários animais no lote de uma vez. Retorna quantos foram movidos.
        Task<int> AdicionarAnimaisAsync(int loteId, IEnumerable<int> animalIds);

        Task RemoverAnimalDoLoteAsync(int loteId, int animalId);

        // Retorna false se ainda houver animais no lote
        Task<bool> RemoverAsync(int id);
    }
}
