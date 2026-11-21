using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SistemaGranja.Domain
{
    // Tipo de movimentação. Os números ficam gravados no banco: não altere os existentes.
    public enum TipoMovimentacaoEstoque
    {
        [Display(Name = "Entrada (compra)")] EntradaCompra = 1,
        [Display(Name = "Saída (consumo)")] SaidaConsumo = 2,
        [Display(Name = "Saída (perda)")] SaidaPerda = 3,
        [Display(Name = "Ajuste de inventário (+)")] AjusteInventarioPositivo = 4,
        [Display(Name = "Ajuste de inventário (-)")] AjusteInventarioNegativo = 5
    }

    public static class TipoMovimentacaoEstoqueExtensions
    {
        public static bool EhEntrada(this TipoMovimentacaoEstoque tipo) =>
            tipo is TipoMovimentacaoEstoque.EntradaCompra or TipoMovimentacaoEstoque.AjusteInventarioPositivo;

        public static string NomeExibicao(this TipoMovimentacaoEstoque tipo)
        {
            var campo = typeof(TipoMovimentacaoEstoque).GetField(tipo.ToString());
            return campo?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? tipo.ToString();
        }

        public static string CorExibicao(this TipoMovimentacaoEstoque tipo) => tipo switch
        {
            TipoMovimentacaoEstoque.EntradaCompra => "bg-success",
            TipoMovimentacaoEstoque.AjusteInventarioPositivo => "bg-success bg-opacity-75",
            TipoMovimentacaoEstoque.SaidaConsumo => "bg-primary",
            TipoMovimentacaoEstoque.SaidaPerda => "bg-danger",
            _ => "bg-secondary"
        };
    }

    public class MovimentacaoEstoqueAlimento : IPertenceAFazenda
    {
        public int Id { get; set; }

        // Fazenda dona do registro. Preenchida automaticamente ao salvar.
        public int FazendaId { get; set; }

        [Display(Name = "Alimento")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione o alimento.")]
        public int TipoAlimentoId { get; set; }

        [Display(Name = "Alimento")]
        public virtual TipoAlimento? TipoAlimento { get; set; }

        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        [Required(ErrorMessage = "Informe a data.")]
        public DateTime DataMovimentacao { get; set; }

        [Display(Name = "Tipo")]
        [EnumDataType(typeof(TipoMovimentacaoEstoque), ErrorMessage = "Selecione o tipo de movimentação.")]
        public TipoMovimentacaoEstoque TipoMovimentacao { get; set; }

        // Sempre positiva. Se é entrada ou saída, quem diz é o Tipo.
        [Display(Name = "Quantidade")]
        [Range(0.001, 1_000_000_000, ErrorMessage = "Informe uma quantidade maior que zero.")]
        public double Quantidade { get; set; }

        // Copiada da unidade do alimento no momento do registro
        [Display(Name = "Unidade")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string UnidadeMedida { get; set; }

        [Display(Name = "Preço unitário (R$)")]
        [Range(0, 1_000_000, ErrorMessage = "Preço inválido.")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PrecoUnitario { get; set; }

        [Display(Name = "Fornecedor / destino")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FornecedorOuDestino { get; set; }

        [Display(Name = "Nota fiscal")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NotaFiscal { get; set; }

        [Display(Name = "Registrado por")]
        public int? FuncionarioId { get; set; }

        [Display(Name = "Registrado por")]
        public virtual Funcionario? Funcionario { get; set; }

        [Display(Name = "Observações")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Observacoes { get; set; }

        // Quantidade com sinal: positiva para entradas, negativa para saídas
        public double QuantidadeComSinal => TipoMovimentacao.EhEntrada() ? Quantidade : -Quantidade;

        public decimal ValorTotal => PrecoUnitario * (decimal)Quantidade;

        public MovimentacaoEstoqueAlimento()
        {
            DataMovimentacao = DateTime.Today;
            TipoMovimentacao = TipoMovimentacaoEstoque.SaidaConsumo;
            UnidadeMedida = string.Empty;
            FornecedorOuDestino = string.Empty;
            NotaFiscal = string.Empty;
            Observacoes = string.Empty;
        }
    }
}
