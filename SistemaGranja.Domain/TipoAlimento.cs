using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    public class TipoAlimento
    {
        public int Id { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "Informe o nome do alimento.")]
        public string Nome { get; set; } // Ex: "Ração Crescimento", "Milho Moído", "Sal Mineral"

        [Display(Name = "Descrição")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Descricao { get; set; }

        [Display(Name = "Unidade de medida")]
        [Required(ErrorMessage = "Informe a unidade de medida.")]
        public string UnidadeMedidaPadrao { get; set; } // Ex: "Kg", "Saco 25 kg", "Litro"

        [Display(Name = "Fabricante")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Fabricante { get; set; }

        [Display(Name = "Código interno")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CodigoInterno { get; set; }

        // Quando o saldo ficar igual ou abaixo deste valor, o sistema avisa. Vazio = sem aviso.
        [Display(Name = "Estoque mínimo")]
        [Range(0, 1_000_000_000, ErrorMessage = "O estoque mínimo não pode ser negativo.")]
        public double? EstoqueMinimo { get; set; }

        public virtual ICollection<MovimentacaoEstoqueAlimento> MovimentacoesEstoque { get; set; }

        public TipoAlimento()
        {
            Nome = string.Empty;
            Descricao = string.Empty;
            UnidadeMedidaPadrao = "Kg";
            Fabricante = string.Empty;
            CodigoInterno = string.Empty;
            MovimentacoesEstoque = new HashSet<MovimentacaoEstoqueAlimento>();
        }
    }
}
