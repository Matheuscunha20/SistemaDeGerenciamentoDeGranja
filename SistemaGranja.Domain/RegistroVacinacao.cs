using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    public class RegistroVacinacao
    {
        public int Id { get; set; }

        [Display(Name = "Animal")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione o animal.")]
        public int AnimalId { get; set; }

        [Display(Name = "Animal")]
        public virtual Animal? Animal { get; set; }

        [Display(Name = "Vacina")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione a vacina.")]
        public int TipoVacinaId { get; set; }

        [Display(Name = "Vacina")]
        public virtual TipoVacina? TipoVacina { get; set; }

        [Display(Name = "Data da vacinação")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        [Required(ErrorMessage = "Informe a data da vacinação.")]
        public DateTime DataVacinacao { get; set; }

        [Display(Name = "Dose aplicada")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DoseAplicada { get; set; }

        [Display(Name = "Lote do frasco")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LoteProdutoVacina { get; set; }

        [Display(Name = "Validade do frasco")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? ValidadeProdutoVacina { get; set; }

        [Display(Name = "Aplicado por")]
        public int? FuncionarioId { get; set; }

        [Display(Name = "Aplicado por")]
        public virtual Funcionario? Funcionario { get; set; }

        [Display(Name = "Próxima dose")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? DataProximaDoseRecomendada { get; set; }

        [Display(Name = "Observações")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Observacoes { get; set; }

        public RegistroVacinacao()
        {
            DataVacinacao = DateTime.Today;
            DoseAplicada = string.Empty;
            LoteProdutoVacina = string.Empty;
            Observacoes = string.Empty;
        }
    }
}
