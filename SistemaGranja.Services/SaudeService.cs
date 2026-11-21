using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class SaudeService : ISaudeService
    {
        private readonly AppDbContext _context;

        public SaudeService(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<RegistroSaude> ConsultaComRelacionamentos()
        {
            return _context.RegistrosSaude
                .Include(r => r.Animal)
                    .ThenInclude(a => a!.Raca)
                        .ThenInclude(r => r!.Especie)
                .Include(r => r.Funcionario);
        }

        public async Task<IEnumerable<RegistroSaude>> ObterHistoricoAsync(
            int? animalId = null, TipoOcorrenciaSaude? tipo = null, DateTime? de = null, DateTime? ate = null, string? busca = null)
        {
            var consulta = ConsultaComRelacionamentos();

            if (animalId.HasValue)
                consulta = consulta.Where(r => r.AnimalId == animalId);

            if (tipo.HasValue)
                consulta = consulta.Where(r => r.TipoOcorrencia == tipo);

            if (de.HasValue)
                consulta = consulta.Where(r => r.DataOcorrencia >= de.Value.Date);

            if (ate.HasValue)
            {
                var fimDoDia = ate.Value.Date.AddDays(1);
                consulta = consulta.Where(r => r.DataOcorrencia < fimDoDia);
            }

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                consulta = consulta.Where(r =>
                    r.Animal!.TagIdentificacao.ToLower().Contains(termo) ||
                    r.Animal.Nome.ToLower().Contains(termo) ||
                    r.Diagnostico.ToLower().Contains(termo) ||
                    r.DescricaoDetalhada.ToLower().Contains(termo));
            }

            return await consulta
                .OrderByDescending(r => r.DataOcorrencia)
                .ThenByDescending(r => r.Id)
                .ToListAsync();
        }

        public async Task<RegistroSaude?> ObterPorIdAsync(int id)
        {
            return await ConsultaComRelacionamentos()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Dictionary<string, int>> ContarAnimaisPorEstadoAsync()
        {
            return await _context.Animais
                .GroupBy(a => a.StatusSaude)
                .Select(grupo => new { Estado = grupo.Key, Quantidade = grupo.Count() })
                .ToDictionaryAsync(x => x.Estado, x => x.Quantidade);
        }

        public async Task AdicionarAsync(RegistroSaude registro, string? novoEstadoDoAnimal)
        {
            if (registro == null)
                throw new ArgumentNullException(nameof(registro));

            Normalizar(registro);
            _context.RegistrosSaude.Add(registro);

            if (!string.IsNullOrWhiteSpace(novoEstadoDoAnimal))
            {
                var animal = await _context.Animais.FindAsync(registro.AnimalId);
                if (animal != null)
                    animal.StatusSaude = novoEstadoDoAnimal.Trim();
            }

            // Ocorrência e estado do animal são salvos juntos, numa única transação
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(RegistroSaude registro)
        {
            if (registro == null)
                throw new ArgumentNullException(nameof(registro));

            Normalizar(registro);
            _context.RegistrosSaude.Update(registro);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var registro = await _context.RegistrosSaude.FindAsync(id);
            if (registro == null) return;

            _context.RegistrosSaude.Remove(registro);
            await _context.SaveChangesAsync();
        }

        private static void Normalizar(RegistroSaude registro)
        {
            registro.DataOcorrencia = registro.DataOcorrencia.Date;
            registro.DescricaoDetalhada = registro.DescricaoDetalhada.Trim();
            registro.Diagnostico = registro.Diagnostico.Trim();
            registro.TratamentoRealizado = registro.TratamentoRealizado.Trim();
            registro.MedicamentosAdministrados = registro.MedicamentosAdministrados.Trim();
            registro.ObservacoesAdicionais = registro.ObservacoesAdicionais.Trim();
        }
    }
}
