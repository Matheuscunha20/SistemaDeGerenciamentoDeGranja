using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class RacaService : IRacaService
    {
        private readonly AppDbContext _context;

        public RacaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Raca>> ObterTodasAsync(int? especieId = null)
        {
            var consulta = _context.Racas.Include(r => r.Especie).AsQueryable();

            if (especieId.HasValue)
                consulta = consulta.Where(r => r.EspecieId == especieId);

            // "Mestiço" e "Outra" ficam sempre no fim da lista
            return await consulta
                .OrderBy(r => r.Especie!.Nome)
                .ThenBy(r => r.Nome == "Outra" ? 2 : r.Nome == "Mestiço" ? 1 : 0)
                .ThenBy(r => r.Nome)
                .ToListAsync();
        }

        public async Task<Raca?> ObterPorIdAsync(int id)
        {
            return await _context.Racas
                .Include(r => r.Especie)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> ExisteNomeAsync(int especieId, string nome, int? ignorarId = null)
        {
            var nomeNormalizado = nome.Trim().ToLower();
            return await _context.Racas.AnyAsync(r =>
                r.EspecieId == especieId && r.Nome.ToLower() == nomeNormalizado && r.Id != ignorarId);
        }

        public async Task<int> ContarAnimaisAsync(int racaId)
        {
            return await _context.Animais.CountAsync(a => a.RacaId == racaId);
        }

        public async Task AdicionarAsync(Raca raca)
        {
            raca.Nome = raca.Nome.Trim();
            _context.Racas.Add(raca);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Raca raca)
        {
            raca.Nome = raca.Nome.Trim();
            _context.Racas.Update(raca);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var raca = await _context.Racas.FindAsync(id);
            if (raca == null) return true;

            if (await _context.Animais.AnyAsync(a => a.RacaId == id))
                return false;

            _context.Racas.Remove(raca);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
