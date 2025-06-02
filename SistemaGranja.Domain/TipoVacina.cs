using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: TipoVacina.cs
using System.Collections.Generic;

namespace SistemaGranja.Domain
{
    public class TipoVacina
    {
        public int Id { get; set; }
        public string NomeVacina { get; set; } // Ex: "Vacina Anti-Aftosa SuperV", "Vacina RaivaVet Plus"
        public string DoencaAlvo { get; set; } // Doença(s) que previne/trata
        public string Fabricante { get; set; }
        public string DosagemPadraoRecomendada { get; set; } // Ex: "2ml", "1 dose"
        public string ViaAdministracaoComum { get; set; } // Ex: "Intramuscular", "Subcutânea"
        public int? IntervaloReforcoDias { get; set; } // Dias para a próxima dose de reforço, se houver

        // Propriedade de navegação para registros de vacinação
        public virtual ICollection<RegistroVacinacao> RegistrosVacinacao { get; set; }


        public TipoVacina()
        {
            NomeVacina = string.Empty;
            DoencaAlvo = string.Empty;
            Fabricante = string.Empty;
            DosagemPadraoRecomendada = string.Empty;
            ViaAdministracaoComum = string.Empty;
            RegistrosVacinacao = new HashSet<RegistroVacinacao>();
        }
    }
}