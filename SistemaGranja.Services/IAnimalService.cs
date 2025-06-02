// Arquivo: IAnimalService.cs
using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IAnimalService
    {
        Task<IEnumerable<Animal>> ObterTodosComRelacionamentosAsync(); // Modificado para incluir relacionamentos
        Task<Animal?> ObterPorIdComRelacionamentosAsync(int id); // Modificado
        Task AdicionarAsync(Animal animal);
        Task AtualizarAsync(Animal animal);
        Task RemoverAsync(int id);
        // Você pode adicionar outros métodos específicos para animais aqui no futuro
    }
}