using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    public class SaidaDeAnimaisService : ISaidaDeAnimaisService
    {
        private readonly AppDbContext _context;

        public SaidaDeAnimaisService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ResultadoSaida> RegistrarSaidaAsync(DadosSaida dados)
        {
            var ids = dados.AnimalIds.Distinct().ToList();
            if (ids.Count == 0) return new ResultadoSaida { Erro = "Selecione pelo menos um animal." };
            if (!Enum.IsDefined(dados.Motivo)) return new ResultadoSaida { Erro = "Motivo inválido." };
            if (dados.Data.Date > DateTime.Today) return new ResultadoSaida { Erro = "A data da saída não pode ser no futuro." };

            var venda = dados.Motivo == MotivoSaidaAnimal.Venda;
            var valorTotal = venda && dados.ValorTotal.HasValue ? Math.Round(dados.ValorTotal.Value, 2) : 0m;
            if (valorTotal < 0) return new ResultadoSaida { Erro = "O valor da venda não pode ser negativo." };

            var animais = await _context.Animais
                .Where(a => a.Ativo && ids.Contains(a.Id))
                .OrderBy(a => a.TagIdentificacao)
                .ToListAsync();
            if (animais.Count == 0) return new ResultadoSaida { Erro = "Nenhum dos animais está no rebanho." };

            // Valor de cada animal: divide o total; os centavos que sobram vão para o último
            var parte = valorTotal > 0 ? Math.Floor(valorTotal / animais.Count * 100) / 100 : 0m;
            for (var i = 0; i < animais.Count; i++)
            {
                var animal = animais[i];
                animal.Ativo = false;
                animal.DataSaida = dados.Data.Date;
                animal.MotivoSaida = dados.Motivo;
                animal.ValorVenda = valorTotal > 0
                    ? (i == animais.Count - 1 ? valorTotal - parte * (animais.Count - 1) : parte)
                    : null;
                animal.GalpaoId = null; // libera a vaga no galpão; o lote fica para o histórico
            }

            LancamentoFinanceiro? receita = null;
            if (venda && valorTotal > 0)
            {
                var categoria = await CategoriasDoSistema.ObterOuCriarAsync(_context, TipoLancamento.Receita, CategoriasDoSistema.VendaDeAnimais);
                var quem = string.IsNullOrWhiteSpace(dados.Comprador) ? "" : $" para {dados.Comprador.Trim()}";
                var descricao = animais.Count == 1
                    ? $"Venda do animal {animais[0].TagIdentificacao}{quem}"
                    : $"Venda de {animais.Count} animais{quem}";
                if (descricao.Length > 200) descricao = descricao.Substring(0, 200);

                var lote = dados.LoteId ?? (animais.Select(a => a.LoteId).Distinct().Count() == 1 ? animais[0].LoteId : null);
                var observacoes = string.Join(" · ", new[]
                {
                    animais.Count > 1 ? "Brincos: " + string.Join(", ", animais.Select(a => a.TagIdentificacao)) : null,
                    string.IsNullOrWhiteSpace(dados.Observacoes) ? null : dados.Observacoes.Trim()
                }.Where(t => t != null));
                if (observacoes.Length > 500) observacoes = observacoes.Substring(0, 497) + "...";

                receita = new LancamentoFinanceiro
                {
                    Tipo = TipoLancamento.Receita,
                    Descricao = descricao,
                    Valor = valorTotal,
                    Data = dados.Data.Date,
                    Pago = dados.Recebido,
                    DataPagamento = dados.Recebido ? dados.Data.Date : null,
                    CategoriaId = categoria.Id,
                    FormaPagamento = Enum.IsDefined(dados.FormaPagamento) ? dados.FormaPagamento : FormaPagamento.Outro,
                    LoteId = lote,
                    Observacoes = observacoes,
                    CriadoEm = DateTime.UtcNow
                };
                _context.LancamentosFinanceiros.Add(receita);
            }

            await _context.SaveChangesAsync();
            return new ResultadoSaida { Quantidade = animais.Count, Receita = receita };
        }

        public async Task<string?> VoltarAoRebanhoAsync(int animalId)
        {
            var animal = await _context.Animais.FindAsync(animalId);
            if (animal == null) return "Animal não encontrado.";
            if (animal.Ativo) return "Este animal já está no rebanho.";

            var tag = animal.TagIdentificacao.Trim().ToLower();
            if (await _context.Animais.AnyAsync(a => a.Ativo && a.Id != animalId && a.TagIdentificacao.ToLower() == tag))
                return $"Já existe outro animal no rebanho com o brinco {animal.TagIdentificacao}.";

            animal.Ativo = true;
            animal.DataSaida = null;
            animal.MotivoSaida = null;
            animal.ValorVenda = null;
            await _context.SaveChangesAsync();
            return null;
        }
    }
}
