using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public class EstoqueService : IEstoqueService
    {
        private const int DiasParaMediaDeConsumo = 30;
        private readonly AppDbContext _context;

        public EstoqueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SaldoAlimento>> ObterSaldosAsync()
        {
            var alimentos = await _context.TiposAlimento
                .OrderBy(t => t.Nome)
                .ToListAsync();

            // Saldo de cada alimento calculado no banco: entradas somam, saídas subtraem
            var saldos = await _context.MovimentacoesEstoqueAlimento
                .GroupBy(m => m.TipoAlimentoId)
                .Select(grupo => new
                {
                    TipoAlimentoId = grupo.Key,
                    Saldo = grupo.Sum(m =>
                        m.TipoMovimentacao == TipoMovimentacaoEstoque.EntradaCompra ||
                        m.TipoMovimentacao == TipoMovimentacaoEstoque.AjusteInventarioPositivo
                            ? m.Quantidade
                            : -m.Quantidade)
                })
                .ToDictionaryAsync(x => x.TipoAlimentoId, x => x.Saldo);

            // Consumo dos últimos 30 dias
            var inicioPeriodo = DateTime.Today.AddDays(-DiasParaMediaDeConsumo);
            var consumos = await _context.MovimentacoesEstoqueAlimento
                .Where(m => m.TipoMovimentacao == TipoMovimentacaoEstoque.SaidaConsumo && m.DataMovimentacao >= inicioPeriodo)
                .GroupBy(m => m.TipoAlimentoId)
                .Select(grupo => new { TipoAlimentoId = grupo.Key, Total = grupo.Sum(m => m.Quantidade) })
                .ToDictionaryAsync(x => x.TipoAlimentoId, x => x.Total);

            // Preço da compra mais recente de cada alimento
            var compras = await _context.MovimentacoesEstoqueAlimento
                .Where(m => m.TipoMovimentacao == TipoMovimentacaoEstoque.EntradaCompra && m.PrecoUnitario > 0)
                .Select(m => new { m.TipoAlimentoId, m.DataMovimentacao, m.Id, m.PrecoUnitario })
                .ToListAsync();
            var ultimosPrecos = compras
                .GroupBy(c => c.TipoAlimentoId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.DataMovimentacao).ThenByDescending(c => c.Id).First().PrecoUnitario);

            return alimentos.Select(alimento => new SaldoAlimento
            {
                Alimento = alimento,
                Saldo = Math.Round(saldos.GetValueOrDefault(alimento.Id), 3),
                ConsumoMedioDiario = consumos.GetValueOrDefault(alimento.Id) / DiasParaMediaDeConsumo,
                UltimoPreco = ultimosPrecos.TryGetValue(alimento.Id, out var preco) ? preco : null
            }).ToList();
        }

        public async Task<double> ObterSaldoAsync(int tipoAlimentoId, int? ignorarMovimentacaoId = null)
        {
            var movimentacoes = await _context.MovimentacoesEstoqueAlimento
                .Where(m => m.TipoAlimentoId == tipoAlimentoId && m.Id != ignorarMovimentacaoId)
                .Select(m => new { m.TipoMovimentacao, m.Quantidade })
                .ToListAsync();

            return Math.Round(movimentacoes.Sum(m => m.TipoMovimentacao.EhEntrada() ? m.Quantidade : -m.Quantidade), 3);
        }

        public async Task<IEnumerable<MovimentacaoEstoqueAlimento>> ObterMovimentacoesAsync(
            int? tipoAlimentoId = null, TipoMovimentacaoEstoque? tipo = null, DateTime? de = null, DateTime? ate = null)
        {
            var consulta = _context.MovimentacoesEstoqueAlimento
                .Include(m => m.TipoAlimento)
                .Include(m => m.Funcionario)
                .AsQueryable();

            if (tipoAlimentoId.HasValue)
                consulta = consulta.Where(m => m.TipoAlimentoId == tipoAlimentoId);

            if (tipo.HasValue)
                consulta = consulta.Where(m => m.TipoMovimentacao == tipo);

            if (de.HasValue)
                consulta = consulta.Where(m => m.DataMovimentacao >= de.Value.Date);

            if (ate.HasValue)
            {
                var fimDoDia = ate.Value.Date.AddDays(1);
                consulta = consulta.Where(m => m.DataMovimentacao < fimDoDia);
            }

            return await consulta
                .OrderByDescending(m => m.DataMovimentacao)
                .ThenByDescending(m => m.Id)
                .ToListAsync();
        }

        public async Task<MovimentacaoEstoqueAlimento?> ObterPorIdAsync(int id)
        {
            // Sem rastreamento: a tela de edição lê o registro e depois salva uma versão nova dele
            return await _context.MovimentacoesEstoqueAlimento
                .AsNoTracking()
                .Include(m => m.TipoAlimento)
                .Include(m => m.Funcionario)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<decimal> ObterGastoComComprasAsync(DateTime de, DateTime ate)
        {
            var fimDoDia = ate.Date.AddDays(1);
            var compras = await _context.MovimentacoesEstoqueAlimento
                .Where(m => m.TipoMovimentacao == TipoMovimentacaoEstoque.EntradaCompra
                            && m.DataMovimentacao >= de.Date && m.DataMovimentacao < fimDoDia)
                .Select(m => new { m.PrecoUnitario, m.Quantidade })
                .ToListAsync();

            return compras.Sum(c => c.PrecoUnitario * (decimal)c.Quantidade);
        }

        public async Task AdicionarAsync(MovimentacaoEstoqueAlimento movimentacao)
        {
            if (movimentacao == null)
                throw new ArgumentNullException(nameof(movimentacao));

            await NormalizarAsync(movimentacao);
            _context.MovimentacoesEstoqueAlimento.Add(movimentacao);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(MovimentacaoEstoqueAlimento movimentacao)
        {
            if (movimentacao == null)
                throw new ArgumentNullException(nameof(movimentacao));

            await NormalizarAsync(movimentacao);
            _context.MovimentacoesEstoqueAlimento.Update(movimentacao);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var movimentacao = await _context.MovimentacoesEstoqueAlimento.FindAsync(id);
            if (movimentacao == null) return;

            _context.MovimentacoesEstoqueAlimento.Remove(movimentacao);
            await _context.SaveChangesAsync();
        }

        private async Task NormalizarAsync(MovimentacaoEstoqueAlimento movimentacao)
        {
            movimentacao.DataMovimentacao = movimentacao.DataMovimentacao.Date;
            movimentacao.Quantidade = Math.Abs(movimentacao.Quantidade);
            movimentacao.FornecedorOuDestino = movimentacao.FornecedorOuDestino.Trim();
            movimentacao.NotaFiscal = movimentacao.NotaFiscal.Trim();
            movimentacao.Observacoes = movimentacao.Observacoes.Trim();

            // Preço só faz sentido em compras
            if (movimentacao.TipoMovimentacao != TipoMovimentacaoEstoque.EntradaCompra)
                movimentacao.PrecoUnitario = 0;

            // A unidade é sempre a do alimento
            var alimento = await _context.TiposAlimento.FindAsync(movimentacao.TipoAlimentoId);
            movimentacao.UnidadeMedida = alimento?.UnidadeMedidaPadrao ?? movimentacao.UnidadeMedida;
        }
    }
}
