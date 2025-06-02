using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Arquivo: Funcionario.cs
using System;
using System.Collections.Generic;

namespace SistemaGranja.Domain
{
    public class Funcionario
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; }
        public string Cpf { get; set; } // Considere adicionar validação e formatação
        public string Rg { get; set; } // Opcional
        public string Cargo { get; set; } // Ex: "Veterinário", "Tratador", "Gerente"
        public DateTime DataAdmissao { get; set; }
        public DateTime? DataDemissao { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public bool Ativo { get; set; } // Para desligamentos

        // Propriedades de navegação para registros feitos pelo funcionário
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
            DataAdmissao = DateTime.Now;
            MovimentacoesEstoqueRegistradas = new HashSet<MovimentacaoEstoqueAlimento>();
            VacinacoesRegistradas = new HashSet<RegistroVacinacao>();
            RegistrosSaudeAtendidos = new HashSet<RegistroSaude>();
        }
    }
}
