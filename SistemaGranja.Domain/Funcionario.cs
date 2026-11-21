using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    public class Funcionario
    {
        public int Id { get; set; }

        [Display(Name = "Nome completo")]
        [Required(ErrorMessage = "Informe o nome completo.")]
        public string NomeCompleto { get; set; }

        // Guardado só com números (ex.: 12345678909). A tela mostra formatado.
        [Display(Name = "CPF")]
        [Required(ErrorMessage = "Informe o CPF.")]
        [Cpf]
        public string Cpf { get; set; }

        [Display(Name = "RG")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Rg { get; set; }

        [Display(Name = "Cargo")]
        [Required(ErrorMessage = "Informe o cargo.")]
        public string Cargo { get; set; }

        [Display(Name = "Data de admissão")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        [Required(ErrorMessage = "Informe a data de admissão.")]
        public DateTime DataAdmissao { get; set; }

        [Display(Name = "Data de desligamento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? DataDemissao { get; set; }

        [Display(Name = "Telefone")]
        [Phone(ErrorMessage = "Telefone inválido.")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Telefone { get; set; }

        [Display(Name = "E-mail")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Email { get; set; }

        [Display(Name = "Situação")]
        public bool Ativo { get; set; }

        [Display(Name = "CPF")]
        public string CpfFormatado => CpfAttribute.Formatar(Cpf);

        // Tempo de casa, ex.: "2 anos e 3 meses"
        [Display(Name = "Tempo de casa")]
        public string TempoDeCasa
        {
            get
            {
                var fim = DataDemissao ?? DateTime.Today;
                if (fim < DataAdmissao) return "—";

                var meses = (fim.Year - DataAdmissao.Year) * 12 + fim.Month - DataAdmissao.Month;
                if (fim.Day < DataAdmissao.Day) meses--;

                if (meses < 1) return "Menos de 1 mês";
                if (meses < 12) return meses == 1 ? "1 mês" : $"{meses} meses";

                var anos = meses / 12;
                var resto = meses % 12;
                var textoAnos = anos == 1 ? "1 ano" : $"{anos} anos";
                if (resto == 0) return textoAnos;
                return resto == 1 ? $"{textoAnos} e 1 mês" : $"{textoAnos} e {resto} meses";
            }
        }

        public virtual ICollection<MovimentacaoEstoqueAlimento> MovimentacoesEstoqueRegistradas { get; set; }
        public virtual ICollection<RegistroVacinacao> VacinacoesRegistradas { get; set; }
        public virtual ICollection<RegistroSaude> RegistrosSaudeAtendidos { get; set; }

        public Funcionario()
        {
            NomeCompleto = string.Empty;
            Cpf = string.Empty;
            Rg = string.Empty;
            Cargo = string.Empty;
            Telefone = string.Empty;
            Email = string.Empty;
            Ativo = true;
            DataAdmissao = DateTime.Today;
            MovimentacoesEstoqueRegistradas = new HashSet<MovimentacaoEstoqueAlimento>();
            VacinacoesRegistradas = new HashSet<RegistroVacinacao>();
            RegistrosSaudeAtendidos = new HashSet<RegistroSaude>();
        }
    }
}
