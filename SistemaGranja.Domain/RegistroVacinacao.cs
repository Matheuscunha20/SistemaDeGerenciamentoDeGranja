using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: RegistroVacinacao.cs
using System;

namespace SistemaGranja.Domain
{
    public class RegistroVacinacao
    {
        public int Id { get; set; }

        public int AnimalId { get; set; } // Chave estrangeira
        public virtual Animal? Animal { get; set; } // Propriedade de navegação

        public int TipoVacinaId { get; set; } // Chave estrangeira
        public virtual TipoVacina? TipoVacina { get; set; } // Propriedade de navegação

        public DateTime DataVacinacao { get; set; }
        public string DoseAplicada { get; set; } // Ex: "2ml", conforme aplicado
        public string LoteProdutoVacina { get; set; } // Lote da vacina (do frasco)
        public DateTime? ValidadeProdutoVacina { get; set; } // Validade do frasco da vacina usada

        public int? FuncionarioId { get; set; } // Chave estrangeira (quem aplicou/registrou)
        public virtual Funcionario? Funcionario { get; set; } // Propriedade de navegação

        public DateTime? DataProximaDoseRecomendada { get; set; }
        public string Observacoes { get; set; }

        public RegistroVacinacao()
        {
            DataVacinacao = DateTime.Now;
            DoseAplicada = string.Empty;
            LoteProdutoVacina = string.Empty;
            Observacoes = string.Empty;
        }
    }
}
