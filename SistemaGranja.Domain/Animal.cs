using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    public class Animal
    {
        public int Id { get; set; }

        [Display(Name = "Brinco / Tag")]
        [Required(ErrorMessage = "Informe o brinco ou tag de identificação.")]
        public string TagIdentificacao { get; set; }

        [Display(Name = "Nome")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Nome { get; set; }

        [Display(Name = "Raça")]
        [Required(ErrorMessage = "Informe a raça.")]
        public string Raca { get; set; }

        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        [Required(ErrorMessage = "Informe a data de nascimento.")]
        public DateTime DataNascimento { get; set; }

        [Display(Name = "Sexo")]
        [Required(ErrorMessage = "Informe o sexo.")]
        public string Sexo { get; set; }

        [Display(Name = "Peso atual (kg)")]
        [Range(0, 5000, ErrorMessage = "O peso deve estar entre {1} e {2} kg.")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public double PesoAtual { get; set; }

        [Display(Name = "Estado de saúde")]
        [Required(ErrorMessage = "Informe o estado de saúde.")]
        public string StatusSaude { get; set; }

        [Display(Name = "Galpão")]
        public int? GalpaoId { get; set; }

        [Display(Name = "Galpão")]
        public virtual Galpao? Galpao { get; set; }

        [Display(Name = "Lote")]
        public int? LoteId { get; set; }

        [Display(Name = "Lote")]
        public virtual Lote? Lote { get; set; }

        public virtual ICollection<RegistroSaude> RegistrosSaude { get; set; }
        public virtual ICollection<RegistroVacinacao> RegistrosVacinacao { get; set; }

        public Animal()
        {
            TagIdentificacao = string.Empty;
            Nome = string.Empty;
            Raca = string.Empty;
            Sexo = string.Empty;
            StatusSaude = "Saudável";
            DataNascimento = DateTime.Today;
            RegistrosSaude = new HashSet<RegistroSaude>();
            RegistrosVacinacao = new HashSet<RegistroVacinacao>();
        }
    }
}
