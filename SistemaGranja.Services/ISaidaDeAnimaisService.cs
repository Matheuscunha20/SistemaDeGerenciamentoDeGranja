using SistemaGranja.Domain;

namespace SistemaGranja.Services
{
    // Dados de uma saída do rebanho (um animal ou vários de um lote)
    public class DadosSaida
    {
        public List<int> AnimalIds { get; set; } = new();
        public MotivoSaidaAnimal Motivo { get; set; } = MotivoSaidaAnimal.Venda;
        public DateTime Data { get; set; } = DateTime.Today;

        // Venda: valor total (dividido entre os animais) e como foi recebido
        public decimal? ValorTotal { get; set; }
        public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Pix;
        public bool Recebido { get; set; } = true;
        public string? Comprador { get; set; }
        public string? Observacoes { get; set; }

        // Lote da venda (a receita fica ligada a ele, para o lucro por lote)
        public int? LoteId { get; set; }
    }

    public class ResultadoSaida
    {
        public int Quantidade { get; set; }
        public LancamentoFinanceiro? Receita { get; set; }
        public string? Erro { get; set; }
    }

    public interface ISaidaDeAnimaisService
    {
        // Tira os animais do rebanho; numa venda com valor, lança a receita no financeiro
        Task<ResultadoSaida> RegistrarSaidaAsync(DadosSaida dados);

        // Desfaz uma saída registrada por engano (a receita, se houver, fica no financeiro para revisar)
        Task<string?> VoltarAoRebanhoAsync(int animalId);
    }
}
