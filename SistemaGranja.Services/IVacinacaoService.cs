using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IVacinacaoService
    {
        // Histórico de aplicações, do mais recente para o mais antigo, com filtros opcionais
        Task<IEnumerable<RegistroVacinacao>> ObterHistoricoAsync(
            int? animalId = null, int? tipoVacinaId = null, DateTime? de = null, DateTime? ate = null, string? busca = null);

        // Próximas doses: para cada animal + vacina, a aplicação mais recente que tem próxima dose
        // marcada até "diasAFrente" dias a partir de hoje (inclui as atrasadas)
        Task<IEnumerable<RegistroVacinacao>> ObterProximasDosesAsync(int diasAFrente = 30);

        Task<RegistroVacinacao?> ObterPorIdAsync(int id);

        // Data sugerida para a próxima dose, a partir do intervalo da vacina
        Task<DateTime?> CalcularProximaDoseAsync(int tipoVacinaId, DateTime dataVacinacao);

        Task AdicionarAsync(RegistroVacinacao registro);

        // Aplica a mesma vacina em vários animais. Retorna quantos registros foram criados.
        Task<int> AdicionarEmGrupoAsync(RegistroVacinacao modelo, IEnumerable<int> animalIds);

        Task AtualizarAsync(RegistroVacinacao registro);
        Task RemoverAsync(int id);
    }
}
