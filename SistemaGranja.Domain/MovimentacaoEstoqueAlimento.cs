using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: MovimentacaoEstoqueAlimento.cs
using System;

namespace SistemaGranja.Domain
{
    // Enum para o tipo de movimentação
    public enum TipoMovimentacaoEstoque
    {
        EntradaCompra = 1,
        SaidaConsumo = 2,
        SaidaPerda = 3,
        AjusteInventarioPositivo = 4,
        AjusteInventarioNegativo = 5
    }

    public class MovimentacaoEstoqueAlimento
    {
        public int Id { get; set; }

        public int TipoAlimentoId { get; set; } // Chave estrangeira
        public virtual TipoAlimento? TipoAlimento { get; set; } // Propriedade de navegação

        public DateTime DataMovimentacao { get; set; }
        public TipoMovimentacaoEstoque TipoMovimentacao { get; set; }
        public double Quantidade { get; set; } // Positivo para entradas, negativo para saídas (ou use campos separados)
        public string UnidadeMedida { get; set; } // Ex: Kg, Sacos (pode ser o padrão do TipoAlimento)
        public decimal PrecoUnitario { get; set; } // Opcional, se quiser rastrear custo
        public string FornecedorOuDestino { get; set; } // Nome do fornecedor na compra, ou destino no consumo
        public string NotaFiscal { get; set; } // Opcional

        public int? FuncionarioId { get; set; } // Chave estrangeira (quem registrou)
        public virtual Funcionario? Funcionario { get; set; } // Propriedade de navegação

        public string Observacoes { get; set; }

        public MovimentacaoEstoqueAlimento()
        {
            DataMovimentacao = DateTime.Now;
            UnidadeMedida = string.Empty;
            FornecedorOuDestino = string.Empty;
            NotaFiscal = string.Empty;
            Observacoes = string.Empty;
        }
    }
}