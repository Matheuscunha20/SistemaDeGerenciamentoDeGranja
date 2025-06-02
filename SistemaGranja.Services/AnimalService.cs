// Arquivo: AnimalService.cs
using Microsoft.EntityFrameworkCore; // Para .Include() e outros
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly AppDbContext _context;

        public AnimalService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Animal animal)
        {
            if (animal == null)
                throw new ArgumentNullException(nameof(animal));
            _context.Animais.Add(animal);
            await _context.SaveChangesAsync();
        }

        public async Task<Animal?> ObterPorIdComRelacionamentosAsync(int id)
        {
            return await _context.Animais
                                 .Include(a => a.Galpao) // Inclui dados do Galpão relacionado
                                 .Include(a => a.Lote)   // Inclui dados do Lote relacionado
                                 .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Animal>> ObterTodosComRelacionamentosAsync()
        {
            return await _context.Animais
                                 .Include(a => a.Galpao) // Inclui dados do Galpão
                                 .Include(a => a.Lote)   // Inclui dados do Lote
                                 .ToListAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var animal = await _context.Animais.FindAsync(id);
            if (animal != null)
            {
                _context.Animais.Remove(animal);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AtualizarAsync(Animal animal)
        {
            if (animal == null)
                throw new ArgumentNullException(nameof(animal));
            _context.Animais.Update(animal);
            await _context.SaveChangesAsync();
        }
    }
}