// Arquivo: LoteService.cs
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
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

        public async Task<IEnumerable<Lote>> ObterTodosAsync()
        {
            return await _context.Lotes.Where(l => l.Ativo).ToListAsync(); // Ex: Listar apenas lotes ativos
        }

        public async Task<Lote?> ObterPorIdAsync(int id)
        {
            return await _context.Lotes.FindAsync(id);
        }
        // Implemente AdicionarAsync, AtualizarAsync, RemoverAsync aqui se necessário
    }
}