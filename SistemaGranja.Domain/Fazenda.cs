using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    // Cada cliente do AgroArca é uma fazenda. Todos os dados pertencem a uma fazenda.
    public class Fazenda
    {
        public int Id { get; set; }

        [Display(Name = "Nome da fazenda")]
        [Required(ErrorMessage = "Informe o nome da fazenda.")]
        [MaxLength(150, ErrorMessage = "Use no máximo {1} caracteres.")]
        public string Nome { get; set; }

        public DateTime DataCriacao { get; set; }

        public Fazenda()
        {
            Nome = string.Empty;
            DataCriacao = DateTime.UtcNow;
        }
    }
}
