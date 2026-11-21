using Microsoft.EntityFrameworkCore;
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

        // Consulta base com galpão, lote, raça e espécie já carregados
        private IQueryable<Animal> ConsultaComRelacionamentos()
        {
            return _context.Animais
                .Include(a => a.Galpao)
                .Include(a => a.Lote)
                .Include(a => a.Raca)
                    .ThenInclude(r => r!.Especie);
        }

        // Só os animais que estão no rebanho (sem os vendidos, mortos etc.)
        public async Task<IEnumerable<Animal>> ObterTodosComRelacionamentosAsync()
        {
            return await ConsultaComRelacionamentos()
                .Where(a => a.Ativo)
                .OrderBy(a => a.TagIdentificacao)
                .ToListAsync();
        }

        public async Task<IEnumerable<Animal>> ObterFiltradosAsync(string? busca, int? especieId, int? galpaoId, string? statusSaude, bool foraDoRebanho = false)
        {
            var consulta = ConsultaComRelacionamentos().Where(a => a.Ativo == !foraDoRebanho);

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

            if (!string.IsNullOrWhiteSpace(statusSaude))
                consulta = consulta.Where(a => a.StatusSaude == statusSaude);

            return foraDoRebanho
                ? await consulta.OrderByDescending(a => a.DataSaida).ThenBy(a => a.TagIdentificacao).ToListAsync()
                : await consulta.OrderBy(a => a.TagIdentificacao).ToListAsync();
        }

        public async Task<Animal?> ObterPorIdComRelacionamentosAsync(int id)
        {
            return await ConsultaComRelacionamentos()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // O brinco é único entre os animais do rebanho (o de um animal vendido pode ser reaproveitado)
        public async Task<bool> ExisteTagAsync(string tag, int? ignorarId = null)
        {
            var tagNormalizada = tag.Trim().ToLower();
            return await _context.Animais
                .AnyAsync(a => a.Ativo && a.TagIdentificacao.ToLower() == tagNormalizada && a.Id != ignorarId);
        }

        public async Task AdicionarAsync(Animal animal)
        {
            if (animal == null)
                throw new ArgumentNullException(nameof(animal));
            animal.TagIdentificacao = animal.TagIdentificacao.Trim();
            _context.Animais.Add(animal);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Animal animal)
        {
            if (animal == null)
                throw new ArgumentNullException(nameof(animal));
            animal.TagIdentificacao = animal.TagIdentificacao.Trim();

            // A tela de edição não mexe na saída do rebanho: mantém o que está gravado
            var gravado = await _context.Animais.AsNoTracking()
                .Where(a => a.Id == animal.Id)
                .Select(a => new { a.Ativo, a.DataSaida, a.MotivoSaida, a.ValorVenda })
                .FirstOrDefaultAsync();
            if (gravado != null)
            {
                animal.Ativo = gravado.Ativo;
                animal.DataSaida = gravado.DataSaida;
                animal.MotivoSaida = gravado.MotivoSaida;
                animal.ValorVenda = gravado.ValorVenda;
            }

            _context.Animais.Update(animal);
            await _context.SaveChangesAsync();
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
    }
}
