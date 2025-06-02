using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: TipoAlimento.cs
using System.Collections.Generic;

namespace SistemaGranja.Domain
{
    public class TipoAlimento
    {
        public int Id { get; set; }
        public string Nome { get; set; } // Ex: "Ração Crescimento XPTO", "Milho Moído Fino", "Sal Mineral Premium"
        public string Descricao { get; set; } // Detalhes, composição, etc.
        public string UnidadeMedidaPadrao { get; set; } // Ex: "Kg", "Saco 25Kg", "Litro"
        public string Fabricante { get; set; }
        public string CodigoInterno { get; set; } // Código de controle seu, se houver

        // Propriedade de navegação para movimentações de estoque
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

