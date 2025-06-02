// Arquivo: GalpaoService.cs (no projeto SistemaGranja.Services)
using Microsoft.EntityFrameworkCore; // Para ToListAsync, FirstOrDefaultAsync, etc.
using SistemaGranja.Data;     // Para AppDbContext
using SistemaGranja.Domain;   // Para Galpao
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class GalpaoService : IGalpaoService
    {
        private readonly AppDbContext _context;

        public GalpaoService(AppDbContext context) // Injeção de Dependência do AppDbContext
        {
            _context = context;
        }

        public async Task AdicionarAsync(Galpao galpao)
        {
            if (galpao == null)
                throw new ArgumentNullException(nameof(galpao));

            _context.Galpoes.Add(galpao);
            await _context.SaveChangesAsync();
        }

        public async Task<Galpao?> ObterPorIdAsync(int id)
        {
            return await _context.Galpoes.FindAsync(id);
        }

        public async Task<IEnumerable<Galpao>> ObterTodosAsync()
        {
            return await _context.Galpoes.ToListAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var galpao = await _context.Galpoes.FindAsync(id);
            if (galpao != null)
            {
                _context.Galpoes.Remove(galpao);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AtualizarAsync(Galpao galpao)
        {
            if (galpao == null)
                throw new ArgumentNullException(nameof(galpao));

            _context.Galpoes.Update(galpao);
            await _context.SaveChangesAsync();
        }
    }
}