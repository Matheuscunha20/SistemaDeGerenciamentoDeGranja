using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class TipoAlimentoService : ITipoAlimentoService
    {
        private readonly AppDbContext _context;

        public TipoAlimentoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoAlimento>> ObterTodosAsync()
        {
            return await _context.TiposAlimento
                .OrderBy(t => t.Nome)
                .ToListAsync();
        }

        public async Task<TipoAlimento?> ObterPorIdAsync(int id)
        {
            return await _context.TiposAlimento.FindAsync(id);
        }

        public async Task<bool> ExisteNomeAsync(string nome, int? ignorarId = null)
        {
            var nomeNormalizado = nome.Trim().ToLower();
            return await _context.TiposAlimento
                .AnyAsync(t => t.Nome.ToLower() == nomeNormalizado && t.Id != ignorarId);
        }

        public async Task<int> ContarMovimentacoesAsync(int tipoAlimentoId)
        {
            return await _context.MovimentacoesEstoqueAlimento
                .CountAsync(m => m.TipoAlimentoId == tipoAlimentoId);
        }

        public async Task AdicionarAsync(TipoAlimento tipoAlimento)
        {
            if (tipoAlimento == null)
                throw new ArgumentNullException(nameof(tipoAlimento));

            tipoAlimento.Nome = tipoAlimento.Nome.Trim();
            _context.TiposAlimento.Add(tipoAlimento);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(TipoAlimento tipoAlimento)
        {
            if (tipoAlimento == null)
                throw new ArgumentNullException(nameof(tipoAlimento));

            tipoAlimento.Nome = tipoAlimento.Nome.Trim();
            _context.TiposAlimento.Update(tipoAlimento);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoverAsync(int id)
        {
            var tipoAlimento = await _context.TiposAlimento.FindAsync(id);
            if (tipoAlimento == null) return true;

            // Excluir apagaria todo o histórico de estoque desse alimento
            if (await _context.MovimentacoesEstoqueAlimento.AnyAsync(m => m.TipoAlimentoId == id))
                return false;

            _context.TiposAlimento.Remove(tipoAlimento);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
