using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    // Raça de uma espécie: Nelore (Bovino), Duroc (Suíno), Cobb (Ave)...
    public class Raca : IPertenceAFazenda
    {
        public int Id { get; set; }

        // Fazenda dona do registro. Preenchida automaticamente ao salvar.
        public int FazendaId { get; set; }

        [Display(Name = "Raça")]
        [Required(ErrorMessage = "Informe o nome da raça.")]
        [MaxLength(100, ErrorMessage = "Use no máximo {1} caracteres.")]
        public string Nome { get; set; }

        [Display(Name = "Espécie")]
        [Required(ErrorMessage = "Selecione a espécie.")]
        public int? EspecieId { get; set; }

        [Display(Name = "Espécie")]
        public virtual Especie? Especie { get; set; }

        public virtual ICollection<Animal> Animais { get; set; }

        public Raca()
        {
            Nome = string.Empty;
            Animais = new HashSet<Animal>();
        }
    }
}
