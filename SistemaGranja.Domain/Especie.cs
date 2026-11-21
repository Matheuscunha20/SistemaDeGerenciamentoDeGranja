using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    // Espécie animal: Bovino, Suíno, Ave, Ovino, Caprino, Equino...
    public class Especie : IPertenceAFazenda
    {
        public int Id { get; set; }

        // Fazenda dona do registro. Preenchida automaticamente ao salvar.
        public int FazendaId { get; set; }

        [Display(Name = "Espécie")]
        [Required(ErrorMessage = "Informe o nome da espécie.")]
        [MaxLength(100, ErrorMessage = "Use no máximo {1} caracteres.")]
        public string Nome { get; set; }

        public virtual ICollection<Raca> Racas { get; set; }

        public Especie()
        {
            Nome = string.Empty;
            Racas = new HashSet<Raca>();
        }
    }
}
