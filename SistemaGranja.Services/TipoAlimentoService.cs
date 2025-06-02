// Arquivo: TipoAlimentoService.cs (no projeto SistemaGranja.Services)
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;     // Para AppDbContext
using SistemaGranja.Domain;   // Para TipoAlimento
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class TipoAlimentoService : ITipoAlimentoService
    {
        private readonly AppDbContext _context;

        public TipoAlimentoService(AppDbContext context) // Injeção de Dependência
        {
            _context = context;
        }

        public async Task AdicionarAsync(TipoAlimento tipoAlimento)
        {
            if (tipoAlimento == null)
                throw new ArgumentNullException(nameof(tipoAlimento));

            _context.TiposAlimento.Add(tipoAlimento);
            await _context.SaveChangesAsync();
        }

        public async Task<TipoAlimento?> ObterPorIdAsync(int id)
        {
            return await _context.TiposAlimento.FindAsync(id);
        }

        public async Task<IEnumerable<TipoAlimento>> ObterTodosAsync()
        {
            return await _context.TiposAlimento.ToListAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var tipoAlimento = await _context.TiposAlimento.FindAsync(id);
            if (tipoAlimento != null)
            {
                _context.TiposAlimento.Remove(tipoAlimento);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AtualizarAsync(TipoAlimento tipoAlimento)
        {
            if (tipoAlimento == null)
                throw new ArgumentNullException(nameof(tipoAlimento));

            _context.TiposAlimento.Update(tipoAlimento);
            await _context.SaveChangesAsync();
        }
    }
}