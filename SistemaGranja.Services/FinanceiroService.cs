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
            var fim = ate.Date.AddDays(1);
            var totais = await _context.LancamentosFinanceiros
                .Where(l => l.Data >= de.Date && l.Data < fim)
                .GroupBy(l => new { l.Tipo, Categoria = l.Categoria!.Nome })
                .Select(g => new { g.Key.Tipo, g.Key.Categoria, Total = g.Sum(l => l.Valor) })
                .ToListAsync();

            return new ResumoFinanceiro
            {
                Receitas = totais.Where(t => t.Tipo == TipoLancamento.Receita).Sum(t => t.Total),
                Despesas = totais.Where(t => t.Tipo == TipoLancamento.Despesa).Sum(t => t.Total),
                PorCategoria = totais
                    .OrderByDescending(t => t.Total)
                    .Select(t => (t.Categoria, t.Tipo, t.Total))
                    .ToList()
            };
        }

        public async Task<string?> SalvarLancamentoAsync(LancamentoFinanceiro lancamento)
        {
            lancamento.Descricao = (lancamento.Descricao ?? string.Empty).Trim();
            lancamento.Observacoes = (lancamento.Observacoes ?? string.Empty).Trim();
            lancamento.Valor = Math.Round(lancamento.Valor, 2);
            lancamento.Data = lancamento.Data.Date;

            if (lancamento.Valor <= 0) return "Informe um valor maior que zero.";
            if (!Enum.IsDefined(lancamento.Tipo)) return "Tipo inválido.";
            if (!Enum.IsDefined(lancamento.FormaPagamento)) return "Forma de pagamento inválida.";

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
                _context.LancamentosFinanceiros.Add(lancamento);
            }
            else
            {
                var existente = await _context.LancamentosFinanceiros.FindAsync(lancamento.Id);
                if (existente == null) return "Lançamento não encontrado.";

                existente.Tipo = lancamento.Tipo;
                existente.Descricao = lancamento.Descricao;
                existente.Valor = lancamento.Valor;
                existente.Data = lancamento.Data;
                existente.CategoriaId = lancamento.CategoriaId;
                existente.FormaPagamento = lancamento.FormaPagamento;
                existente.LoteId = lancamento.LoteId;
                existente.Observacoes = lancamento.Observacoes;
            }

            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<bool> RemoverLancamentoAsync(int id)
        {
            var lancamento = await _context.LancamentosFinanceiros.FindAsync(id);
            if (lancamento == null) return false;

            _context.LancamentosFinanceiros.Remove(lancamento);
            await _context.SaveChangesAsync();
            return true;
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
