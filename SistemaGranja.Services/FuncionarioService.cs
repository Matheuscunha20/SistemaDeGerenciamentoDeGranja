using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly AppDbContext _context;

        public FuncionarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Funcionario>> ObterTodosAsync(bool? ativo = null, string? busca = null)
        {
            var consulta = _context.Funcionarios.AsQueryable();

            if (ativo.HasValue)
                consulta = consulta.Where(f => f.Ativo == ativo.Value);

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                var termoCpf = CpfAttribute.SomenteNumeros(busca);
                consulta = consulta.Where(f =>
                    f.NomeCompleto.ToLower().Contains(termo) ||
                    f.Cargo.ToLower().Contains(termo) ||
                    (termoCpf.Length > 0 && f.Cpf.Contains(termoCpf)));
            }

            return await consulta
                .OrderByDescending(f => f.Ativo)
                .ThenBy(f => f.NomeCompleto)
                .ToListAsync();
        }

        public async Task<Funcionario?> ObterPorIdAsync(int id)
        {
            return await _context.Funcionarios.FindAsync(id);
        }

        public async Task<bool> ExisteCpfAsync(string cpf, int? ignorarId = null)
        {
            var numeros = CpfAttribute.SomenteNumeros(cpf);
            return await _context.Funcionarios.AnyAsync(f => f.Cpf == numeros && f.Id != ignorarId);
        }

        public async Task<int> ContarRegistrosAsync(int funcionarioId)
        {
            var estoque = await _context.MovimentacoesEstoqueAlimento.CountAsync(m => m.FuncionarioId == funcionarioId);
            var vacinas = await _context.RegistrosVacinacao.CountAsync(r => r.FuncionarioId == funcionarioId);
            var saude = await _context.RegistrosSaude.CountAsync(r => r.FuncionarioId == funcionarioId);
            return estoque + vacinas + saude;
        }

        public async Task AdicionarAsync(Funcionario funcionario)
        {
            if (funcionario == null)
                throw new ArgumentNullException(nameof(funcionario));

            Normalizar(funcionario);
            funcionario.Ativo = true;
            funcionario.DataDemissao = null;
            _context.Funcionarios.Add(funcionario);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Funcionario funcionario)
        {
            if (funcionario == null)
                throw new ArgumentNullException(nameof(funcionario));

            Normalizar(funcionario);
            _context.Funcionarios.Update(funcionario);
            await _context.SaveChangesAsync();
        }

        public async Task DesligarAsync(int id, DateTime dataDesligamento)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null) return;

            funcionario.Ativo = false;
            funcionario.DataDemissao = dataDesligamento.Date;
            await _context.SaveChangesAsync();
        }

        public async Task ReativarAsync(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null) return;

            funcionario.Ativo = true;
            funcionario.DataDemissao = null;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null) return true;

            if (await ContarRegistrosAsync(id) > 0)
                return false;

            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();
            return true;
        }

        private static void Normalizar(Funcionario funcionario)
        {
            funcionario.NomeCompleto = funcionario.NomeCompleto.Trim();
            funcionario.Cpf = CpfAttribute.SomenteNumeros(funcionario.Cpf);
            funcionario.Email = funcionario.Email.Trim().ToLower();
            funcionario.Telefone = funcionario.Telefone.Trim();
            funcionario.Rg = funcionario.Rg.Trim();
        }
    }
}
