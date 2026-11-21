using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SistemaGranja.Domain
{
    // Tipo de ocorrência de saúde. Os números ficam gravados no banco: não altere os existentes.
    public enum TipoOcorrenciaSaude
    {
        [Display(Name = "Doença diagnosticada")] DoencaDiagnosticada = 1,
        [Display(Name = "Ferimento")] Ferimento = 2,
        [Display(Name = "Início de tratamento")] TratamentoIniciado = 3,
        [Display(Name = "Fim de tratamento")] TratamentoConcluido = 4,
        [Display(Name = "Exame")] ExameRealizado = 5,
        [Display(Name = "Parto")] Parto = 6,
        [Display(Name = "Observação de rotina")] ObservacaoRotina = 7,
        [Display(Name = "Outro")] Outro = 99
    }

    public static class TipoOcorrenciaSaudeExtensions
    {
        // Nome em português definido no [Display] de cada opção
        public static string NomeExibicao(this TipoOcorrenciaSaude tipo)
        {
            var campo = typeof(TipoOcorrenciaSaude).GetField(tipo.ToString());
            return campo?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? tipo.ToString();
        }

        // Cor do selo na tela (classes do Bootstrap)
        public static string CorExibicao(this TipoOcorrenciaSaude tipo) => tipo switch
        {
            TipoOcorrenciaSaude.DoencaDiagnosticada => "bg-danger",
            TipoOcorrenciaSaude.Ferimento => "bg-warning text-dark",
            TipoOcorrenciaSaude.TratamentoIniciado => "bg-primary",
            TipoOcorrenciaSaude.TratamentoConcluido => "bg-success",
            TipoOcorrenciaSaude.ExameRealizado => "bg-info text-dark",
            TipoOcorrenciaSaude.Parto => "bg-secondary",
            _ => "bg-light text-dark border"
        };
    }

    public class RegistroSaude : IPertenceAFazenda
    {
        public int Id { get; set; }

        // Fazenda dona do registro. Preenchida automaticamente ao salvar.
        public int FazendaId { get; set; }

        [Display(Name = "Animal")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione o animal.")]
        public int AnimalId { get; set; }

        [Display(Name = "Animal")]
        public virtual Animal? Animal { get; set; }

        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        [Required(ErrorMessage = "Informe a data.")]
        public DateTime DataOcorrencia { get; set; }

        [Display(Name = "Tipo de ocorrência")]
        [EnumDataType(typeof(TipoOcorrenciaSaude), ErrorMessage = "Selecione o tipo de ocorrência.")]
        public TipoOcorrenciaSaude TipoOcorrencia { get; set; }

        [Display(Name = "O que aconteceu")]
        [Required(ErrorMessage = "Descreva a ocorrência (sintomas, detalhes do ferimento, resultado do exame...).")]
        public string DescricaoDetalhada { get; set; }

        [Display(Name = "Diagnóstico")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Diagnostico { get; set; }

        [Display(Name = "Tratamento realizado")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TratamentoRealizado { get; set; }

        [Display(Name = "Medicamentos")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MedicamentosAdministrados { get; set; }

        [Display(Name = "Atendido por")]
        public int? FuncionarioId { get; set; }

        [Display(Name = "Atendido por")]
        public virtual Funcionario? Funcionario { get; set; }

        [Display(Name = "Observações")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ObservacoesAdicionais { get; set; }

        public RegistroSaude()
        {
            DataOcorrencia = DateTime.Today;
            TipoOcorrencia = TipoOcorrenciaSaude.ObservacaoRotina;
            DescricaoDetalhada = string.Empty;
            Diagnostico = string.Empty;
            TratamentoRealizado = string.Empty;
            MedicamentosAdministrados = string.Empty;
            ObservacoesAdicionais = string.Empty;
        }
    }
}
