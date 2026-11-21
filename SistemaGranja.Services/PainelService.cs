using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    // Junta os números de todos os módulos para o painel inicial
    public class PainelService : IPainelService
    {
        private readonly AppDbContext _context;
        private readonly IGalpaoService _galpaoService;
        private readonly IVacinacaoService _vacinacaoService;
        private readonly IEstoqueService _estoqueService;

        public PainelService(
            AppDbContext context,
            IGalpaoService galpaoService,
            IVacinacaoService vacinacaoService,
            IEstoqueService estoqueService)
        {
            _context = context;
            _galpaoService = galpaoService;
            _vacinacaoService = vacinacaoService;
            _estoqueService = estoqueService;
        }

        public async Task<ResumoPainel> ObterResumoAsync()
        {
            var resumo = new ResumoPainel
            {
                TotalAnimais = await _context.Animais.CountAsync(a => a.Ativo),
                LotesAtivos = await _context.Lotes.CountAsync(l => l.Ativo),
                FuncionariosAtivos = await _context.Funcionarios.CountAsync(f => f.Ativo)
            };

            // Animais por espécie (os sem raça entram como "Não informada")
            var porEspecie = await _context.Animais
                .Where(a => a.Ativo)
                .GroupBy(a => a.Raca!.Especie!.Nome)
                .Select(g => new { Especie = g.Key, Quantidade = g.Count() })
                .ToListAsync();
            resumo.AnimaisPorEspecie = porEspecie
                .OrderByDescending(x => x.Quantidade)
                .ToDictionary(x => x.Especie ?? "Não informada", x => x.Quantidade);

            resumo.AnimaisPorEstadoDeSaude = await _context.Animais
                .Where(a => a.Ativo)
                .GroupBy(a => a.StatusSaude)
                .Select(g => new { Estado = g.Key, Quantidade = g.Count() })
                .ToDictionaryAsync(x => x.Estado, x => x.Quantidade);

            // Galpões: ocupação total e quais estão lotados
            var galpoes = await _galpaoService.ObterTodosAsync();
            var ocupacao = await _galpaoService.ObterOcupacaoAsync();
            resumo.TotalGalpoes = galpoes.Count();
            resumo.CapacidadeTotal = galpoes.Sum(g => g.CapacidadeMaxima);
            resumo.AnimaisAlojados = ocupacao.Values.Sum();
            resumo.GalpoesLotados = galpoes
                .Where(g => ocupacao.GetValueOrDefault(g.Id) >= g.CapacidadeMaxima)
                .Select(g => g.NomeOuNumero)
                .ToList();

            // Vacinação: doses atrasadas e dos próximos 7 dias
            var hoje = DateTime.Today;
            var proximasDoses = await _vacinacaoService.ObterProximasDosesAsync(7);
            resumo.VacinasAtrasadas = proximasDoses.Count(r => r.DataProximaDoseRecomendada!.Value.Date < hoje);
            resumo.VacinasProximos7Dias = proximasDoses.Count() - resumo.VacinasAtrasadas;
            resumo.ProximasDoses = proximasDoses.Take(5).ToList();

            // Estoque: alimentos baixos ou zerados
            var saldos = await _estoqueService.ObterSaldosAsync();
            resumo.AlimentosComEstoqueBaixo = saldos
                .Where(s => s.Situacao != SituacaoEstoque.Normal)
                .OrderBy(s => s.Saldo)
                .ToList();

            // Zerados primeiro; depois os que duram menos dias; quem não tem consumo registrado fica por último
            resumo.EstoqueAcabandoPrimeiro = saldos
                .OrderBy(s => s.Situacao == SituacaoEstoque.Zerado ? 0 : 1)
                .ThenBy(s => s.DiasRestantes ?? double.MaxValue)
                .ThenBy(s => s.Alimento.Nome)
                .Take(4)
                .ToList();

            // Contas a pagar em aberto: vencidas e dos próximos 7 dias
            var daquiASete = hoje.AddDays(8);
            var contasAPagar = await _context.LancamentosFinanceiros
                .Where(l => !l.Pago && l.Tipo == TipoLancamento.Despesa && l.Data < daquiASete)
                .Select(l => new { l.Data, l.Valor })
                .ToListAsync();
            resumo.ContasAPagarVencidas = contasAPagar.Count(c => c.Data < hoje);
            resumo.ValorContasAPagarVencidas = contasAPagar.Where(c => c.Data < hoje).Sum(c => c.Valor);
            resumo.ContasAPagarProximos7Dias = contasAPagar.Count(c => c.Data >= hoje);
            resumo.APagarProximos7Dias = contasAPagar.Where(c => c.Data >= hoje).Sum(c => c.Valor);

            resumo.AReceberProximos7Dias = await _context.LancamentosFinanceiros
                .Where(l => !l.Pago && l.Tipo == TipoLancamento.Receita && l.Data >= hoje && l.Data < daquiASete)
                .SumAsync(l => l.Valor);

            // Pago e recebido neste mês
            var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);
            var inicioDoProximoMes = inicioDoMes.AddMonths(1);
            var totaisDoMes = await _context.LancamentosFinanceiros
                .Where(l => l.Pago && l.DataPagamento >= inicioDoMes && l.DataPagamento < inicioDoProximoMes)
                .GroupBy(l => l.Tipo)
                .Select(g => new { Tipo = g.Key, Total = g.Sum(l => l.Valor) })
                .ToListAsync();
            resumo.RecebidoNoMes = totaisDoMes.Where(t => t.Tipo == TipoLancamento.Receita).Sum(t => t.Total);
            resumo.PagoNoMes = totaisDoMes.Where(t => t.Tipo == TipoLancamento.Despesa).Sum(t => t.Total);

            return resumo;
        }
    }
}
