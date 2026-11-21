using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    public class Animal : IPertenceAFazenda
    {
        public int Id { get; set; }

        // Fazenda dona do registro. Preenchida automaticamente ao salvar.
        public int FazendaId { get; set; }

        [Display(Name = "Brinco / Tag")]
        [Required(ErrorMessage = "Informe o brinco ou tag de identificação.")]
        public string TagIdentificacao { get; set; }

        [Display(Name = "Nome")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Nome { get; set; }

        [Display(Name = "Raça")]
        [Required(ErrorMessage = "Selecione a raça.")]
        public int? RacaId { get; set; }

        [Display(Name = "Raça")]
        public virtual Raca? Raca { get; set; }

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

        // Idade calculada a partir da data de nascimento (não vai para o banco)
        [Display(Name = "Idade")]
        public string IdadeDescricao
        {
            get
            {
                var hoje = DateTime.Today;
                if (DataNascimento > hoje) return "—";

                var meses = (hoje.Year - DataNascimento.Year) * 12 + hoje.Month - DataNascimento.Month;
                if (hoje.Day < DataNascimento.Day) meses--;

                if (meses < 1)
                {
                    var dias = (hoje - DataNascimento.Date).Days;
                    return dias == 1 ? "1 dia" : $"{dias} dias";
                }
                if (meses < 12) return meses == 1 ? "1 mês" : $"{meses} meses";

                var anos = meses / 12;
                var resto = meses % 12;
                var textoAnos = anos == 1 ? "1 ano" : $"{anos} anos";
                if (resto == 0) return textoAnos;
                return resto == 1 ? $"{textoAnos} e 1 mês" : $"{textoAnos} e {resto} meses";
            }
        }

        public virtual ICollection<RegistroSaude> RegistrosSaude { get; set; }
        public virtual ICollection<RegistroVacinacao> RegistrosVacinacao { get; set; }

        public Animal()
        {
            TagIdentificacao = string.Empty;
            Nome = string.Empty;
            Sexo = string.Empty;
            StatusSaude = "Saudável";
            DataNascimento = DateTime.Today;
            RegistrosSaude = new HashSet<RegistroSaude>();
            RegistrosVacinacao = new HashSet<RegistroVacinacao>();
        }
    }
}
