using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    public class TipoVacina
    {
        public int Id { get; set; }

        [Display(Name = "Vacina")]
        [Required(ErrorMessage = "Informe o nome da vacina.")]
        public string NomeVacina { get; set; } // Ex: "Raiva", "Clostridioses (polivalente)"

        [Display(Name = "Doença(s) que previne")]
        [Required(ErrorMessage = "Informe a doença que a vacina previne.")]
        public string DoencaAlvo { get; set; }

        [Display(Name = "Fabricante")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Fabricante { get; set; }

        [Display(Name = "Dose padrão")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DosagemPadraoRecomendada { get; set; } // Ex: "2 ml", "1 dose"

        [Display(Name = "Via de aplicação")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ViaAdministracaoComum { get; set; } // Ex: "Intramuscular", "Subcutânea"

        // Usado para sugerir a data da próxima dose. Vazio = dose única.
        [Display(Name = "Reforço a cada (dias)")]
        [Range(1, 3650, ErrorMessage = "O intervalo deve estar entre {1} e {2} dias.")]
        public int? IntervaloReforcoDias { get; set; }

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
