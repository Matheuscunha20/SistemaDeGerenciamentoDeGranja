using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: Animal.cs (revisado para incluir Galpao e Lote)
using System;
using System.Collections.Generic; // Para futuras coleções se necessário

namespace SistemaGranja.Domain
{
    public class Animal
    {
        public int Id { get; set; }
        public string TagIdentificacao { get; set; }
        public string Nome { get; set; }
        public string Raca { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Sexo { get; set; }
        public double PesoAtual { get; set; }
        public string StatusSaude { get; set; }

        // Chave estrangeira para Galpao
        public int? GalpaoId { get; set; } // Nullable se o animal puder estar sem galpão definido
        public virtual Galpao? Galpao { get; set; } // Propriedade de navegação

        // Chave estrangeira para Lote
        public int? LoteId { get; set; } // Nullable se o animal puder não pertencer a um lote
        public virtual Lote? Lote { get; set; } // Propriedade de navegação

        // Propriedades de navegação para registros associados
        public virtual ICollection<RegistroSaude> RegistrosSaude { get; set; }
        public virtual ICollection<RegistroVacinacao> RegistrosVacinacao { get; set; }


        public Animal()
        {
            TagIdentificacao = string.Empty;
            Nome = string.Empty;
            Raca = string.Empty;
            Sexo = string.Empty;
            StatusSaude = "Saudável";
            DataNascimento = DateTime.MinValue;
            RegistrosSaude = new HashSet<RegistroSaude>();
            RegistrosVacinacao = new HashSet<RegistroVacinacao>();
        }
    }
}