using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface ISaudeService
    {
        // Ocorrências da mais recente para a mais antiga, com filtros opcionais
        Task<IEnumerable<RegistroSaude>> ObterHistoricoAsync(
            int? animalId = null, TipoOcorrenciaSaude? tipo = null, DateTime? de = null, DateTime? ate = null, string? busca = null);

        Task<RegistroSaude?> ObterPorIdAsync(int id);

        // Quantos animais existem em cada estado de saúde: chave = "Saudável", "Em tratamento"...
        Task<Dictionary<string, int>> ContarAnimaisPorEstadoAsync();

        // Registra a ocorrência e, se informado, muda o estado de saúde do animal
        Task AdicionarAsync(RegistroSaude registro, string? novoEstadoDoAnimal);

        Task AtualizarAsync(RegistroSaude registro);
        Task RemoverAsync(int id);
    }
}
