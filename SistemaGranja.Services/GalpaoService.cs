using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class GalpaoService : IGalpaoService
    {
        private readonly AppDbContext _context;

        public GalpaoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Galpao>> ObterTodosAsync()
        {
            return await _context.Galpoes
                .OrderBy(g => g.NomeOuNumero)
                .ToListAsync();
        }

        public async Task<Galpao?> ObterPorIdAsync(int id)
        {
            return await _context.Galpoes.FindAsync(id);
        }

        public async Task<Galpao?> ObterComAnimaisAsync(int id)
        {
            return await _context.Galpoes
                .Include(g => g.Animais.Where(a => a.Ativo))
                    .ThenInclude(a => a.Raca)
                        .ThenInclude(r => r!.Especie)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Dictionary<int, int>> ObterOcupacaoAsync()
        {
            return await _context.Animais
                .Where(a => a.Ativo && a.GalpaoId != null)
                .GroupBy(a => a.GalpaoId!.Value)
                .Select(grupo => new { GalpaoId = grupo.Key, Quantidade = grupo.Count() })
                .ToDictionaryAsync(x => x.GalpaoId, x => x.Quantidade);
        }

        public async Task<int> ContarAnimaisAsync(int galpaoId, int? ignorarAnimalId = null)
        {
            return await _context.Animais
                .CountAsync(a => a.Ativo && a.GalpaoId == galpaoId && a.Id != ignorarAnimalId);
        }

        public async Task<bool> ExisteNomeAsync(string nome, int? ignorarId = null)
        {
            var nomeNormalizado = nome.Trim().ToLower();
            return await _context.Galpoes
                .AnyAsync(g => g.NomeOuNumero.ToLower() == nomeNormalizado && g.Id != ignorarId);
        }

        public async Task AdicionarAsync(Galpao galpao)
        {
            if (galpao == null)
                throw new ArgumentNullException(nameof(galpao));

            galpao.NomeOuNumero = galpao.NomeOuNumero.Trim();
            _context.Galpoes.Add(galpao);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Galpao galpao)
        {
            if (galpao == null)
                throw new ArgumentNullException(nameof(galpao));

            galpao.NomeOuNumero = galpao.NomeOuNumero.Trim();
            _context.Galpoes.Update(galpao);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var galpao = await _context.Galpoes.FindAsync(id);
            if (galpao == null) return true;

            if (await _context.Animais.AnyAsync(a => a.GalpaoId == id))
                return false;

            _context.Galpoes.Remove(galpao);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
