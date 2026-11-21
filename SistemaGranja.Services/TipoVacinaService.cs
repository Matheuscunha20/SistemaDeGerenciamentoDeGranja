using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class TipoVacinaService : ITipoVacinaService
    {
        private readonly AppDbContext _context;

        public TipoVacinaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoVacina>> ObterTodosAsync()
        {
            return await _context.TiposVacina
                .OrderBy(t => t.NomeVacina)
                .ToListAsync();
        }

        public async Task<TipoVacina?> ObterPorIdAsync(int id)
        {
            return await _context.TiposVacina.FindAsync(id);
        }

        public async Task<bool> ExisteNomeAsync(string nome, int? ignorarId = null)
        {
            var nomeNormalizado = nome.Trim().ToLower();
            return await _context.TiposVacina
                .AnyAsync(t => t.NomeVacina.ToLower() == nomeNormalizado && t.Id != ignorarId);
        }

        public async Task<Dictionary<int, int>> ObterQuantidadeAplicacoesAsync()
        {
            return await _context.RegistrosVacinacao
                .GroupBy(r => r.TipoVacinaId)
                .Select(grupo => new { TipoVacinaId = grupo.Key, Quantidade = grupo.Count() })
                .ToDictionaryAsync(x => x.TipoVacinaId, x => x.Quantidade);
        }

        public async Task AdicionarAsync(TipoVacina tipoVacina)
        {
            if (tipoVacina == null)
                throw new ArgumentNullException(nameof(tipoVacina));

            tipoVacina.NomeVacina = tipoVacina.NomeVacina.Trim();
            _context.TiposVacina.Add(tipoVacina);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(TipoVacina tipoVacina)
        {
            if (tipoVacina == null)
                throw new ArgumentNullException(nameof(tipoVacina));

            tipoVacina.NomeVacina = tipoVacina.NomeVacina.Trim();
            _context.TiposVacina.Update(tipoVacina);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var tipoVacina = await _context.TiposVacina.FindAsync(id);
            if (tipoVacina == null) return true;

            // No banco, excluir a vacina apagaria todas as aplicações dela (cascata)
            if (await _context.RegistrosVacinacao.AnyAsync(r => r.TipoVacinaId == id))
                return false;

            _context.TiposVacina.Remove(tipoVacina);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
