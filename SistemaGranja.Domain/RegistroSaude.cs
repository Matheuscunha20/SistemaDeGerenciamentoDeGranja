using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: RegistroSaude.cs
using System;

namespace SistemaGranja.Domain
{
    // Enum para o tipo de ocorrência de saúde
    public enum TipoOcorrenciaSaude
    {
        DoencaDiagnosticada = 1,
        Ferimento = 2,
        TratamentoIniciado = 3,
        TratamentoConcluido = 4,
        ExameRealizado = 5,
        Parto = 6,
        ObservacaoRotina = 7,
        Outro = 99
    }

    public class RegistroSaude
    {
        public int Id { get; set; }

        public int AnimalId { get; set; } // Chave estrangeira
        public virtual Animal? Animal { get; set; } // Propriedade de navegação

        public DateTime DataOcorrencia { get; set; }
        public TipoOcorrenciaSaude TipoOcorrencia { get; set; }
        public string DescricaoDetalhada { get; set; } // Sintomas, detalhes do ferimento, etc.
        public string Diagnostico { get; set; } // Se aplicável
        public string TratamentoRealizado { get; set; } // Se aplicável
        public string MedicamentosAdministrados { get; set; } // Lista ou descrição

        public int? FuncionarioId { get; set; } // Chave estrangeira (quem atendeu/registrou)
        public virtual Funcionario? Funcionario { get; set; } // Propriedade de navegação

        public string ObservacoesAdicionais { get; set; }

        public RegistroSaude()
        {
            DataOcorrencia = DateTime.Now;
            DescricaoDetalhada = string.Empty;
            Diagnostico = string.Empty;
            TratamentoRealizado = string.Empty;
            MedicamentosAdministrados = string.Empty;
            ObservacoesAdicionais = string.Empty;
        }
    }
}