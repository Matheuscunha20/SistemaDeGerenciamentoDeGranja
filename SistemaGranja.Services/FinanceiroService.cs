using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    public class FinanceiroService : IFinanceiroService
    {
        private readonly AppDbContext _context;

        public FinanceiroService(AppDbContext context)
        {
            _context = context;
        }

        // ---------- Lançamentos ----------

        public const int MaximoDeRepeticoes = 36;

        public async Task<List<LancamentoFinanceiro>> ObterLancamentosAsync(FiltroLancamentos filtro)
        {
            var consulta = _context.LancamentosFinanceiros
                .Include(l => l.Categoria)
                .Include(l => l.Lote)
                .AsNoTracking()
                .AsQueryable();

            if (filtro.De.HasValue) consulta = consulta.Where(l => l.Data >= filtro.De.Value.Date);
            if (filtro.Ate.HasValue)
            {
                var fim = filtro.Ate.Value.Date.AddDays(1);
                consulta = consulta.Where(l => l.Data < fim);
            }
            if (filtro.Tipo.HasValue) consulta = consulta.Where(l => l.Tipo == filtro.Tipo.Value);
            if (filtro.CategoriaId.HasValue) consulta = consulta.Where(l => l.CategoriaId == filtro.CategoriaId.Value);
            if (filtro.Pago.HasValue) consulta = consulta.Where(l => l.Pago == filtro.Pago.Value);
            if (!string.IsNullOrWhiteSpace(filtro.Busca))
            {
                var termo = filtro.Busca.Trim().ToLower();
                consulta = consulta.Where(l => l.Descricao.ToLower().Contains(termo) || l.Observacoes.ToLower().Contains(termo));
            }

            return await consulta
                .OrderByDescending(l => l.Data)
                .ThenByDescending(l => l.Id)
                .ToListAsync();
        }

        public async Task<LancamentoFinanceiro?> ObterLancamentoAsync(int id)
        {
            return await _context.LancamentosFinanceiros
                .Include(l => l.Categoria)
                .Include(l => l.Lote)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<ResumoFinanceiro> ObterResumoAsync(DateTime de, DateTime ate)
        {
            var inicio = de.Date;
            var fim = ate.Date.AddDays(1);

            // Realizado: pago/recebido dentro do período
            var realizados = await _context.LancamentosFinanceiros
                .Where(l => l.Pago && l.DataPagamento >= inicio && l.DataPagamento < fim)
                .GroupBy(l => new { l.Tipo, Categoria = l.Categoria!.Nome })
                .Select(g => new { g.Key.Tipo, g.Key.Categoria, Total = g.Sum(l => l.Valor) })
                .ToListAsync();

            // Em aberto: vencimento dentro do período
            var emAberto = await _context.LancamentosFinanceiros
                .Where(l => !l.Pago && l.Data >= inicio && l.Data < fim)
                .GroupBy(l => l.Tipo)
                .Select(g => new { Tipo = g.Key, Total = g.Sum(l => l.Valor) })
                .ToListAsync();

            return new ResumoFinanceiro
            {
                Receitas = realizados.Where(t => t.Tipo == TipoLancamento.Receita).Sum(t => t.Total),
                Despesas = realizados.Where(t => t.Tipo == TipoLancamento.Despesa).Sum(t => t.Total),
                AReceber = emAberto.Where(t => t.Tipo == TipoLancamento.Receita).Sum(t => t.Total),
                APagar = emAberto.Where(t => t.Tipo == TipoLancamento.Despesa).Sum(t => t.Total),
                PorCategoria = realizados
                    .OrderByDescending(t => t.Total)
                    .Select(t => (t.Categoria, t.Tipo, t.Total))
                    .ToList()
            };
        }

        public async Task<string?> SalvarLancamentoAsync(LancamentoFinanceiro lancamento, int repetirMeses = 1)
        {
            lancamento.Descricao = (lancamento.Descricao ?? string.Empty).Trim();
            lancamento.Observacoes = (lancamento.Observacoes ?? string.Empty).Trim();
            lancamento.Valor = Math.Round(lancamento.Valor, 2);
            lancamento.Data = lancamento.Data.Date;
            NormalizarPagamento(lancamento);

            if (lancamento.Valor <= 0) return "Informe um valor maior que zero.";
            if (!Enum.IsDefined(lancamento.Tipo)) return "Tipo inválido.";
            if (!Enum.IsDefined(lancamento.FormaPagamento)) return "Forma de pagamento inválida.";
            if (repetirMeses < 1 || repetirMeses > MaximoDeRepeticoes)
                return $"Repita de 1 a {MaximoDeRepeticoes} meses.";

            // A categoria precisa existir nesta fazenda e ser do mesmo tipo (receita/despesa)
            var categoria = lancamento.CategoriaId.HasValue
                ? await _context.CategoriasFinanceiras.FindAsync(lancamento.CategoriaId.Value)
                : null;
            if (categoria == null) return "Selecione a categoria.";
            if (categoria.Tipo != lancamento.Tipo)
                return $"A categoria \"{categoria.Nome}\" é de {categoria.Tipo.NomeExibicao().ToLower()}.";

            if (lancamento.LoteId.HasValue && await _context.Lotes.FindAsync(lancamento.LoteId.Value) == null)
                return "Lote não encontrado.";

            if (lancamento.Id == 0)
            {
                lancamento.CriadoEm = DateTime.UtcNow;
                if (repetirMeses == 1)
                {
                    _context.LancamentosFinanceiros.Add(lancamento);
                }
                else
                {
                    // Uma conta por mês: "Descrição (1/3)", "(2/3)"... A primeira mantém a situação
                    // escolhida; as seguintes ficam em aberto, vencendo no mesmo dia dos meses seguintes.
                    var descricao = lancamento.Descricao;
                    if (descricao.Length > 190) descricao = descricao.Substring(0, 190);
                    for (var i = 0; i < repetirMeses; i++)
                    {
                        var parcela = i == 0 ? lancamento : new LancamentoFinanceiro
                        {
                            Tipo = lancamento.Tipo,
                            Valor = lancamento.Valor,
                            Data = lancamento.Data.AddMonths(i),
                            CategoriaId = lancamento.CategoriaId,
                            FormaPagamento = lancamento.FormaPagamento,
                            LoteId = lancamento.LoteId,
                            Observacoes = lancamento.Observacoes,
                            Pago = false,
                            CriadoEm = lancamento.CriadoEm
                        };
                        parcela.Descricao = $"{descricao} ({i + 1}/{repetirMeses})";
                        _context.LancamentosFinanceiros.Add(parcela);
                    }
                }
            }
            else
            {
                var existente = await _context.LancamentosFinanceiros.FindAsync(lancamento.Id);
                if (existente == null) return "Lançamento não encontrado.";

                existente.Tipo = lancamento.Tipo;
                existente.Descricao = lancamento.Descricao;
                existente.Valor = lancamento.Valor;
                existente.Data = lancamento.Data;
                existente.Pago = lancamento.Pago;
                existente.DataPagamento = lancamento.DataPagamento;
                existente.CategoriaId = lancamento.CategoriaId;
                existente.FormaPagamento = lancamento.FormaPagamento;
                existente.LoteId = lancamento.LoteId;
                existente.Observacoes = lancamento.Observacoes;
            }

            await _context.SaveChangesAsync();
            return null;
        }

        // Pago sem data de pagamento: usa a própria data. Em aberto: sem data de pagamento.
        private static void NormalizarPagamento(LancamentoFinanceiro lancamento)
        {
            if (lancamento.Pago)
                lancamento.DataPagamento = (lancamento.DataPagamento ?? lancamento.Data).Date;
            else
                lancamento.DataPagamento = null;
        }

        public async Task<bool> RemoverLancamentoAsync(int id)
        {
            var lancamento = await _context.LancamentosFinanceiros.FindAsync(id);
            if (lancamento == null) return false;

            _context.LancamentosFinanceiros.Remove(lancamento);
            await _context.SaveChangesAsync();
            return true;
        }

        // ---------- Contas a pagar e a receber ----------

        public async Task<ContasEmAberto> ObterContasEmAbertoAsync(TipoLancamento? tipo, DateTime hoje)
        {
            var consulta = _context.LancamentosFinanceiros
                .Include(l => l.Categoria)
                .Include(l => l.Lote)
                .AsNoTracking()
                .Where(l => !l.Pago);
            if (tipo.HasValue) consulta = consulta.Where(l => l.Tipo == tipo.Value);

            var contas = await consulta.OrderBy(l => l.Data).ThenBy(l => l.Id).ToListAsync();
            var dia = hoje.Date;
            return new ContasEmAberto
            {
                Vencidas = contas.Where(l => l.Data < dia).ToList(),
                ProximosSeteDias = contas.Where(l => l.Data >= dia && l.Data <= dia.AddDays(7)).ToList(),
                Depois = contas.Where(l => l.Data > dia.AddDays(7)).ToList()
            };
        }

        public async Task<string?> MarcarComoPagoAsync(int id, DateTime dataPagamento)
        {
            var lancamento = await _context.LancamentosFinanceiros.FindAsync(id);
            if (lancamento == null) return "Conta não encontrada.";
            if (lancamento.Pago) return "Esta conta já estava paga.";

            lancamento.Pago = true;
            lancamento.DataPagamento = dataPagamento.Date;
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<FluxoDeCaixa> ObterFluxoDeCaixaAsync(DateTime hoje, int mesesAntes = 2, int mesesDepois = 3)
        {
            var mesAtual = new DateTime(hoje.Year, hoje.Month, 1);
            var primeiroMes = mesAtual.AddMonths(-mesesAntes);
            var fimDoPeriodo = mesAtual.AddMonths(mesesDepois + 1);

            var saldoInicial = await _context.LancamentosFinanceiros
                .Where(l => l.Pago && l.DataPagamento < primeiroMes)
                .SumAsync(l => l.Tipo == TipoLancamento.Receita ? l.Valor : -l.Valor);

            var proximoDia = hoje.Date.AddDays(1);
            var saldoAtual = await _context.LancamentosFinanceiros
                .Where(l => l.Pago && l.DataPagamento < proximoDia)
                .SumAsync(l => l.Tipo == TipoLancamento.Receita ? l.Valor : -l.Valor);

            var realizados = await _context.LancamentosFinanceiros
                .Where(l => l.Pago && l.DataPagamento >= primeiroMes && l.DataPagamento < fimDoPeriodo)
                .Select(l => new { l.Tipo, l.Valor, Data = l.DataPagamento!.Value })
                .ToListAsync();

            // Em aberto até o fim do período (as vencidas entram no mês atual: ainda vão acontecer)
            var previstos = await _context.LancamentosFinanceiros
                .Where(l => !l.Pago && l.Data < fimDoPeriodo)
                .Select(l => new { l.Tipo, l.Valor, l.Data })
                .ToListAsync();

            var fluxo = new FluxoDeCaixa { SaldoInicial = saldoInicial, SaldoAtual = saldoAtual };
            var acumulado = saldoInicial;
            for (var mes = primeiroMes; mes < fimDoPeriodo; mes = mes.AddMonths(1))
            {
                var proximo = mes.AddMonths(1);
                var noMes = realizados.Where(r => r.Data >= mes && r.Data < proximo).ToList();
                var inicioDoMes = mes;
                var previstosNoMes = previstos
                    .Where(p => inicioDoMes >= mesAtual
                        && (inicioDoMes == mesAtual ? p.Data < proximo : p.Data >= inicioDoMes && p.Data < proximo))
                    .ToList();

                var item = new FluxoMes
                {
                    Mes = mes,
                    Passado = mes < mesAtual,
                    Atual = mes == mesAtual,
                    EntradasRealizadas = noMes.Where(r => r.Tipo == TipoLancamento.Receita).Sum(r => r.Valor),
                    SaidasRealizadas = noMes.Where(r => r.Tipo == TipoLancamento.Despesa).Sum(r => r.Valor),
                    EntradasPrevistas = previstosNoMes.Where(p => p.Tipo == TipoLancamento.Receita).Sum(p => p.Valor),
                    SaidasPrevistas = previstosNoMes.Where(p => p.Tipo == TipoLancamento.Despesa).Sum(p => p.Valor)
                };
                acumulado += item.SaldoDoMes;
                item.SaldoAcumulado = acumulado;
                fluxo.Meses.Add(item);
            }
            return fluxo;
        }

        // ---------- Categorias ----------

        public Task GarantirCategoriasPadraoAsync() => SemeadorDeDados.SemearCategoriasFinanceirasAsync(_context);

        public async Task<List<CategoriaFinanceira>> ObterCategoriasAsync(TipoLancamento? tipo = null, bool somenteAtivas = false)
        {
            var consulta = _context.CategoriasFinanceiras.AsNoTracking().AsQueryable();
            if (tipo.HasValue) consulta = consulta.Where(c => c.Tipo == tipo.Value);
            if (somenteAtivas) consulta = consulta.Where(c => c.Ativa);

            return await consulta
                .OrderBy(c => c.Tipo)
                .ThenBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task<CategoriaFinanceira?> ObterCategoriaAsync(int id)
        {
            return await _context.CategoriasFinanceiras.FindAsync(id);
        }

        public async Task<int> ContarLancamentosDaCategoriaAsync(int categoriaId)
        {
            return await _context.LancamentosFinanceiros.CountAsync(l => l.CategoriaId == categoriaId);
        }

        public async Task<string?> SalvarCategoriaAsync(CategoriaFinanceira categoria)
        {
            categoria.Nome = (categoria.Nome ?? string.Empty).Trim();
            if (categoria.Nome.Length == 0) return "Informe o nome da categoria.";
            if (!Enum.IsDefined(categoria.Tipo)) return "Tipo inválido.";

            var nome = categoria.Nome.ToLower();
            var repetida = await _context.CategoriasFinanceiras
                .AnyAsync(c => c.Tipo == categoria.Tipo && c.Nome.ToLower() == nome && c.Id != categoria.Id);
            if (repetida) return $"Já existe uma categoria de {categoria.Tipo.NomeExibicao().ToLower()} com esse nome.";

            if (categoria.Id == 0)
            {
                _context.CategoriasFinanceiras.Add(categoria);
            }
            else
            {
                var existente = await _context.CategoriasFinanceiras.FindAsync(categoria.Id);
                if (existente == null) return "Categoria não encontrada.";

                // O tipo não muda depois de criada: os lançamentos dela já são daquele tipo
                if (existente.Tipo != categoria.Tipo && await ContarLancamentosDaCategoriaAsync(existente.Id) > 0)
                    return "Esta categoria já tem lançamentos, então não dá para trocar entre receita e despesa.";

                existente.Nome = categoria.Nome;
                existente.Tipo = categoria.Tipo;
                existente.Ativa = categoria.Ativa;
            }

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> RemoverCategoriaAsync(int id)
        {
            var categoria = await _context.CategoriasFinanceiras.FindAsync(id);
            if (categoria == null) return "Categoria não encontrada.";

            var usos = await ContarLancamentosDaCategoriaAsync(id);
            if (usos > 0)
                return $"A categoria \"{categoria.Nome}\" tem {usos} lançamento(s) e não pode ser excluída. Você pode desativá-la.";

            _context.CategoriasFinanceiras.Remove(categoria);
            await _context.SaveChangesAsync();
            return null;
        }
    }
}
