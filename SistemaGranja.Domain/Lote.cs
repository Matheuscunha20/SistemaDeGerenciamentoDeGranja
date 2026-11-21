using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    // Agrupamento de animais para manejo conjunto.
    // Ex.: "BEZERRAS-MAIO", "ENGORDA-2026-A", "MATRIZES-LACTACAO"
    public class Lote : IPertenceAFazenda
    {
        public int Id { get; set; }

        // Fazenda dona do registro. Preenchida automaticamente ao salvar.
        public int FazendaId { get; set; }

        [Display(Name = "Código do lote")]
        [Required(ErrorMessage = "Informe o código do lote.")]
        public string CodigoLote { get; set; }

        [Display(Name = "Data de criação")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        [Required(ErrorMessage = "Informe a data de criação.")]
        public DateTime DataCriacao { get; set; }

        [Display(Name = "Descrição")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Descricao { get; set; }

        // Lote encerrado não aparece mais na escolha de lote dos animais
        [Display(Name = "Situação")]
        public bool Ativo { get; set; }

        public virtual ICollection<Animal> Animais { get; set; }

        public Lote()
        {
            CodigoLote = string.Empty;
            Descricao = string.Empty;
            Ativo = true;
            DataCriacao = DateTime.Today;
            Animais = new HashSet<Animal>();
        }
    }
}
