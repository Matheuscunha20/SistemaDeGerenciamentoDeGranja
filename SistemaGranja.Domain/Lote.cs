using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: Lote.cs
using System;
using System.Collections.Generic;

namespace SistemaGranja.Domain
{
    public class Lote
    {
        public int Id { get; set; }
        public string CodigoLote { get; set; } // Ex: "LOTE2025-A", "BEZERRAS-MAIO"
        public DateTime DataCriacao { get; set; }
        public string Descricao { get; set; } // Ex: "Lote para engorda final", "Bezerras desmamadas"
        public bool Ativo { get; set; } // Indica se o lote está ativo ou já foi "fechado"

        // Propriedade de navegação: um lote pode ter vários animais
        public virtual ICollection<Animal> Animais { get; set; }

        public Lote()
        {
            CodigoLote = string.Empty;
            Descricao = string.Empty;
            Ativo = true;
            DataCriacao = DateTime.Now;
            Animais = new HashSet<Animal>();
        }
    }
}