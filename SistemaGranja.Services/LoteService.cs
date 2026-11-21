using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class LoteService : ILoteService
    {
        private readonly AppDbContext _context;

        public LoteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lote>> ObterTodosAsync(bool? ativo = null)
        {
            var consulta = _context.Lotes.AsQueryable();

            if (ativo.HasValue)
                consulta = consulta.Where(l => l.Ativo == ativo.Value);

            return await consulta
                .OrderByDescending(l => l.Ativo)
                .ThenByDescending(l => l.DataCriacao)
                .ThenBy(l => l.CodigoLote)
                .ToListAsync();
        }

        public async Task<IEnumerable<Lote>> ObterAtivosAsync()
        {
            return await _context.Lotes
                .Where(l => l.Ativo)
                .OrderBy(l => l.CodigoLote)
                .ToListAsync();
        }

        public async Task<Lote?> ObterPorIdAsync(int id)
        {
            return await _context.Lotes.FindAsync(id);
        }

        public async Task<Lote?> ObterComAnimaisAsync(int id)
        {
            return await _context.Lotes
                .Include(l => l.Animais)
                    .ThenInclude(a => a.Raca)
                        .ThenInclude(r => r!.Especie)
                .Include(l => l.Animais)
                    .ThenInclude(a => a.Galpao)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Dictionary<int, int>> ObterQuantidadeAnimaisAsync()
        {
            return await _context.Animais
                .Where(a => a.LoteId != null)
                .GroupBy(a => a.LoteId!.Value)
                .Select(grupo => new { LoteId = grupo.Key, Quantidade = grupo.Count() })
                .ToDictionaryAsync(x => x.LoteId, x => x.Quantidade);
        }

        public async Task<IEnumerable<Animal>> ObterAnimaisForaDoLoteAsync(int loteId, string? busca, int? especieId, int? galpaoId)
        {
            var consulta = _context.Animais
                .Include(a => a.Raca)
                    .ThenInclude(r => r!.Especie)
                .Include(a => a.Galpao)
                .Include(a => a.Lote)
                .Where(a => a.LoteId == null || a.LoteId != loteId);

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                consulta = consulta.Where(a =>
                    a.TagIdentificacao.ToLower().Contains(termo) ||
                    a.Nome.ToLower().Contains(termo));
            }

            if (especieId.HasValue)
                consulta = consulta.Where(a => a.Raca != null && a.Raca.EspecieId == especieId);

            if (galpaoId.HasValue)
                consulta = consulta.Where(a => a.GalpaoId == galpaoId);

            return await consulta
                .OrderBy(a => a.TagIdentificacao)
                .ToListAsync();
        }

        public async Task<bool> ExisteCodigoAsync(string codigo, int? ignorarId = null)
        {
            var codigoNormalizado = codigo.Trim().ToLower();
            return await _context.Lotes
                .AnyAsync(l => l.CodigoLote.ToLower() == codigoNormalizado && l.Id != ignorarId);
        }

        public async Task AdicionarAsync(Lote lote)
        {
            if (lote == null)
                throw new ArgumentNullException(nameof(lote));

            lote.CodigoLote = lote.CodigoLote.Trim().ToUpper();
            _context.Lotes.Add(lote);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Lote lote)
        {
            if (lote == null)
                throw new ArgumentNullException(nameof(lote));

            lote.CodigoLote = lote.CodigoLote.Trim().ToUpper();
            _context.Lotes.Update(lote);
            await _context.SaveChangesAsync();
        }

        public async Task AlterarSituacaoAsync(int id, bool ativo)
        {
            var lote = await _context.Lotes.FindAsync(id);
            if (lote == null) return;

            lote.Ativo = ativo;
            await _context.SaveChangesAsync();
        }

        public async Task<int> AdicionarAnimaisAsync(int loteId, IEnumerable<int> animalIds)
        {
            var ids = animalIds.Distinct().ToList();
            if (ids.Count == 0) return 0;

            var animais = await _context.Animais
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();

            foreach (var animal in animais)
                animal.LoteId = loteId;

            await _context.SaveChangesAsync();
            return animais.Count;
        }

        public async Task RemoverAnimalDoLoteAsync(int loteId, int animalId)
        {
            var animal = await _context.Animais
                .FirstOrDefaultAsync(a => a.Id == animalId && a.LoteId == loteId);
            if (animal == null) return;

            animal.LoteId = null;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var lote = await _context.Lotes.FindAsync(id);
            if (lote == null) return true;

            if (await _context.Animais.AnyAsync(a => a.LoteId == id))
                return false;

            _context.Lotes.Remove(lote);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
