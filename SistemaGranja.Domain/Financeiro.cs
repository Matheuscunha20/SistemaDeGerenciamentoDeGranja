using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace SistemaGranja.Domain
{
    // Os números ficam gravados no banco: não altere os existentes.
    public enum TipoLancamento
    {
        [Display(Name = "Receita")] Receita = 1,
        [Display(Name = "Despesa")] Despesa = 2
    }

    public enum FormaPagamento
    {
        [Display(Name = "Pix")] Pix = 1,
        [Display(Name = "Dinheiro")] Dinheiro = 2,
        [Display(Name = "Cartão de débito")] CartaoDebito = 3,
        [Display(Name = "Cartão de crédito")] CartaoCredito = 4,
        [Display(Name = "Boleto")] Boleto = 5,
        [Display(Name = "Transferência")] Transferencia = 6,
        [Display(Name = "Cheque")] Cheque = 7,
        [Display(Name = "Outro")] Outro = 99
    }

    public static class FinanceiroExtensions
    {
        public static string NomeExibicao(this TipoLancamento tipo) => NomeDoEnum(tipo);
        public static string NomeExibicao(this FormaPagamento forma) => NomeDoEnum(forma);

        private static string NomeDoEnum<T>(T valor) where T : struct, Enum
        {
            var campo = typeof(T).GetField(valor.ToString());
            return campo?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? valor.ToString();
        }
    }

    // Categoria de receita ou despesa (ex.: "Ração", "Venda de animais")
    public class CategoriaFinanceira : IPertenceAFazenda
    {
        public int Id { get; set; }

        public int FazendaId { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "Informe o nome da categoria.")]
        [MaxLength(80, ErrorMessage = "Use no máximo {1} caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Tipo")]
        public TipoLancamento Tipo { get; set; } = TipoLancamento.Despesa;

        // Categoria desativada some das opções, mas os lançamentos antigos continuam com ela
        [Display(Name = "Ativa")]
        public bool Ativa { get; set; } = true;
    }

    // Uma receita (dinheiro que entrou) ou despesa (dinheiro que saiu)
    public class LancamentoFinanceiro : IPertenceAFazenda
    {
        public int Id { get; set; }

        public int FazendaId { get; set; }

        [Display(Name = "Tipo")]
        public TipoLancamento Tipo { get; set; } = TipoLancamento.Despesa;

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "Informe uma descrição.")]
        [MaxLength(200, ErrorMessage = "Use no máximo {1} caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Valor (R$)")]
        [Range(0.01, 9_999_999_999.0, ErrorMessage = "Informe um valor maior que zero.")]
        [Column(TypeName = "decimal(12,2)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Valor { get; set; }

        // Data em que o dinheiro entra/sai; numa conta em aberto, é o vencimento
        [Display(Name = "Data / vencimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Data { get; set; } = DateTime.Today;

        // false = conta a pagar (despesa) ou a receber (receita) ainda em aberto
        [Display(Name = "Já foi pago / recebido")]
        public bool Pago { get; set; } = true;

        [Display(Name = "Pago / recebido em")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DataPagamento { get; set; }

        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "Selecione a categoria.")]
        public int? CategoriaId { get; set; }

        [Display(Name = "Categoria")]
        public virtual CategoriaFinanceira? Categoria { get; set; }

        [Display(Name = "Forma de pagamento")]
        public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Pix;

        // Opcional: liga o lançamento a um lote, para o custo/lucro por lote
        [Display(Name = "Lote (opcional)")]
        public int? LoteId { get; set; }

        [Display(Name = "Lote")]
        public virtual Lote? Lote { get; set; }

        [Display(Name = "Observações")]
        [MaxLength(500, ErrorMessage = "Use no máximo {1} caracteres.")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Observacoes { get; set; } = string.Empty;

        // Criado automaticamente por uma compra no estoque (atualiza junto com ela)
        public int? MovimentacaoEstoqueId { get; set; }
        public virtual MovimentacaoEstoqueAlimento? MovimentacaoEstoque { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        // Valor com sinal: receita soma, despesa subtrai
        public decimal ValorComSinal => Tipo == TipoLancamento.Receita ? Valor : -Valor;

        // Conta em aberto com vencimento já passado
        public bool Vencida(DateTime hoje) => !Pago && Data.Date < hoje.Date;
    }
}
