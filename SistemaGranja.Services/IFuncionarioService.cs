using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IFuncionarioService
    {
        // ativo: true = só ativos, false = só desligados, null = todos
        Task<IEnumerable<Funcionario>> ObterTodosAsync(bool? ativo = null, string? busca = null);
        Task<Funcionario?> ObterPorIdAsync(int id);
        Task<bool> ExisteCpfAsync(string cpf, int? ignorarId = null);

        // Quantos registros (estoque, vacinação, saúde) o funcionário fez
        Task<int> ContarRegistrosAsync(int funcionarioId);

        Task AdicionarAsync(Funcionario funcionario);
        Task AtualizarAsync(Funcionario funcionario);
        Task DesligarAsync(int id, DateTime dataDesligamento);
        Task ReativarAsync(int id);

        // Retorna false se o funcionário tiver registros ligados a ele
        Task<bool> RemoverAsync(int id);
    }
}
