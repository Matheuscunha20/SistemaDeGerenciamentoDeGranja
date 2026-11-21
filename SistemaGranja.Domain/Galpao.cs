using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    public class Galpao
    {
        public int Id { get; set; }

        [Display(Name = "Nome ou número")]
        [Required(ErrorMessage = "Informe o nome ou número do galpão.")]
        public string NomeOuNumero { get; set; }

        [Display(Name = "Tipo de instalação")]
        [Required(ErrorMessage = "Informe o tipo de instalação.")]
        public string TipoInstalacao { get; set; } // Ex: "Engorda", "Maternidade", "Creche", "Quarentena"

        [Display(Name = "Capacidade máxima")]
        [Range(1, 100000, ErrorMessage = "A capacidade deve estar entre {1} e {2} animais.")]
        public int CapacidadeMaxima { get; set; }

        [Display(Name = "Localização")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocalizacaoDescricao { get; set; } // Ex: "Setor Norte", "Próximo ao pasto 3"

        [Display(Name = "Observações")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Observacoes { get; set; }

        public virtual ICollection<Animal> Animais { get; set; }

        public Galpao()
        {
            NomeOuNumero = string.Empty;
            TipoInstalacao = string.Empty;
            LocalizacaoDescricao = string.Empty;
            Observacoes = string.Empty;
            Animais = new HashSet<Animal>();
        }
    }
}
