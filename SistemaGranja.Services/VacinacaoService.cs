using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class VacinacaoService : IVacinacaoService
    {
        private readonly AppDbContext _context;

        public VacinacaoService(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<RegistroVacinacao> ConsultaComRelacionamentos()
        {
            return _context.RegistrosVacinacao
                .Include(r => r.Animal)
                    .ThenInclude(a => a!.Raca)
                        .ThenInclude(r => r!.Especie)
                .Include(r => r.Animal)
                    .ThenInclude(a => a!.Galpao)
                .Include(r => r.TipoVacina)
                .Include(r => r.Funcionario);
        }

        public async Task<IEnumerable<RegistroVacinacao>> ObterHistoricoAsync(
            int? animalId = null, int? tipoVacinaId = null, DateTime? de = null, DateTime? ate = null, string? busca = null)
        {
            var consulta = ConsultaComRelacionamentos();

            if (animalId.HasValue)
                consulta = consulta.Where(r => r.AnimalId == animalId);

            if (tipoVacinaId.HasValue)
                consulta = consulta.Where(r => r.TipoVacinaId == tipoVacinaId);

            if (de.HasValue)
                consulta = consulta.Where(r => r.DataVacinacao >= de.Value.Date);

            if (ate.HasValue)
            {
                var fimDoDia = ate.Value.Date.AddDays(1);
                consulta = consulta.Where(r => r.DataVacinacao < fimDoDia);
            }

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                consulta = consulta.Where(r =>
                    r.Animal!.TagIdentificacao.ToLower().Contains(termo) ||
                    r.Animal.Nome.ToLower().Contains(termo));
            }

            return await consulta
                .OrderByDescending(r => r.DataVacinacao)
                .ThenByDescending(r => r.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegistroVacinacao>> ObterProximasDosesAsync(int diasAFrente = 30)
        {
            var limite = DateTime.Today.AddDays(diasAFrente);

            // Busca todas as aplicações e, em memória, fica com a mais recente de cada animal + vacina.
            // Assim, se o reforço já foi dado, o aviso antigo some.
            var registros = await ConsultaComRelacionamentos()
                .AsNoTracking()
                .ToListAsync();

            return registros
                .GroupBy(r => new { r.AnimalId, r.TipoVacinaId })
                .Select(grupo => grupo.OrderByDescending(r => r.DataVacinacao).ThenByDescending(r => r.Id).First())
                .Where(r => r.DataProximaDoseRecomendada.HasValue && r.DataProximaDoseRecomendada.Value.Date <= limite)
                .Where(r => r.Animal == null || r.Animal.Ativo) // animal vendido/morto não precisa de reforço
                .OrderBy(r => r.DataProximaDoseRecomendada)
                .ToList();
        }

        public async Task<RegistroVacinacao?> ObterPorIdAsync(int id)
        {
            return await ConsultaComRelacionamentos()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<DateTime?> CalcularProximaDoseAsync(int tipoVacinaId, DateTime dataVacinacao)
        {
            var tipo = await _context.TiposVacina.FindAsync(tipoVacinaId);
            if (tipo?.IntervaloReforcoDias is int dias && dias > 0)
                return dataVacinacao.Date.AddDays(dias);
            return null;
        }

        public async Task AdicionarAsync(RegistroVacinacao registro)
        {
            if (registro == null)
                throw new ArgumentNullException(nameof(registro));

            Normalizar(registro);
            _context.RegistrosVacinacao.Add(registro);
            await _context.SaveChangesAsync();
        }

        public async Task<int> AdicionarEmGrupoAsync(RegistroVacinacao modelo, IEnumerable<int> animalIds)
        {
            var ids = animalIds.Distinct().ToList();
            if (ids.Count == 0) return 0;

            // Só cria registros para animais que existem de fato
            var idsExistentes = await _context.Animais
                .Where(a => a.Ativo && ids.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();

            Normalizar(modelo);
            foreach (var animalId in idsExistentes)
            {
                _context.RegistrosVacinacao.Add(new RegistroVacinacao
                {
                    AnimalId = animalId,
                    TipoVacinaId = modelo.TipoVacinaId,
                    DataVacinacao = modelo.DataVacinacao,
                    DoseAplicada = modelo.DoseAplicada,
                    LoteProdutoVacina = modelo.LoteProdutoVacina,
                    ValidadeProdutoVacina = modelo.ValidadeProdutoVacina,
                    FuncionarioId = modelo.FuncionarioId,
                    DataProximaDoseRecomendada = modelo.DataProximaDoseRecomendada,
                    Observacoes = modelo.Observacoes
                });
            }

            await _context.SaveChangesAsync();
            return idsExistentes.Count;
        }

        public async Task AtualizarAsync(RegistroVacinacao registro)
        {
            if (registro == null)
                throw new ArgumentNullException(nameof(registro));

            Normalizar(registro);
            _context.RegistrosVacinacao.Update(registro);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var registro = await _context.RegistrosVacinacao.FindAsync(id);
            if (registro == null) return;

            _context.RegistrosVacinacao.Remove(registro);
            await _context.SaveChangesAsync();
        }

        private static void Normalizar(RegistroVacinacao registro)
        {
            registro.DataVacinacao = registro.DataVacinacao.Date;
            registro.DataProximaDoseRecomendada = registro.DataProximaDoseRecomendada?.Date;
            registro.ValidadeProdutoVacina = registro.ValidadeProdutoVacina?.Date;
            registro.DoseAplicada = registro.DoseAplicada.Trim();
            registro.LoteProdutoVacina = registro.LoteProdutoVacina.Trim();
            registro.Observacoes = registro.Observacoes.Trim();
        }
    }
}
