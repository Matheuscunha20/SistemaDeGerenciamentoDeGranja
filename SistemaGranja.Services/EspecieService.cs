using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class EspecieService : IEspecieService
    {
        private readonly AppDbContext _context;

        public EspecieService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Especie>> ObterTodasAsync()
        {
            return await _context.Especies
                .Include(e => e.Racas)
                .OrderBy(e => e.Nome)
                .ToListAsync();
        }

        public async Task<Especie?> ObterPorIdAsync(int id)
        {
            return await _context.Especies
                .Include(e => e.Racas)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> ExisteNomeAsync(string nome, int? ignorarId = null)
        {
            var nomeNormalizado = nome.Trim().ToLower();
            return await _context.Especies
                .AnyAsync(e => e.Nome.ToLower() == nomeNormalizado && e.Id != ignorarId);
        }

        public async Task AdicionarAsync(Especie especie)
        {
            especie.Nome = especie.Nome.Trim();
            _context.Especies.Add(especie);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Especie especie)
        {
            especie.Nome = especie.Nome.Trim();
            _context.Especies.Update(especie);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var especie = await _context.Especies.FindAsync(id);
            if (especie == null) return true;

            if (await _context.Racas.AnyAsync(r => r.EspecieId == id))
                return false;

            _context.Especies.Remove(especie);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
