using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: Galpao.cs
using System.Collections.Generic; // Necessário para ICollection

namespace SistemaGranja.Domain
{
    public class Galpao
    {
        public int Id { get; set; }
        public string NomeOuNumero { get; set; }
        public string TipoInstalacao { get; set; } // Ex: "Engorda", "Maternidade", "Creche", "Quarentena"
        public int CapacidadeMaxima { get; set; }
        public string LocalizacaoDescricao { get; set; } // Ex: "Setor Norte", "Próximo ao pasto 3"
        public string Observacoes { get; set; }

        // Propriedade de navegação: um galpão pode ter vários animais
        public virtual ICollection<Animal> Animais { get; set; }

        public Galpao()
        {
            NomeOuNumero = string.Empty;
            TipoInstalacao = string.Empty;
            LocalizacaoDescricao = string.Empty;
            Observacoes = string.Empty;
            Animais = new HashSet<Animal>(); // Inicializa a coleção
        }
    }
}