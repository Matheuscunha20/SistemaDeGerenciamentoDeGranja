using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaGranja.Domain
{
    // Convite para alguém entrar na equipe da fazenda.
    // O dono gera um link com o Token e envia para a pessoa.
    public class Convite : IPertenceAFazenda
    {
        public const int DiasDeValidade = 7;

        public int Id { get; set; }

        public int FazendaId { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Informe o e-mail da pessoa.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [MaxLength(256)]
        public string Email { get; set; }

        [Display(Name = "Perfil")]
        public PerfilUsuario Perfil { get; set; }

        // Código secreto do link do convite
        [MaxLength(64)]
        public string Token { get; set; }

        public DateTime CriadoEm { get; set; }
        public DateTime ExpiraEm { get; set; }
        public DateTime? AceitoEm { get; set; }

        public bool Expirado => AceitoEm == null && DateTime.UtcNow > ExpiraEm;
        public bool Pendente => AceitoEm == null && !Expirado;

        public Convite()
        {
            Email = string.Empty;
            Token = string.Empty;
            Perfil = PerfilUsuario.Funcionario;
            CriadoEm = DateTime.UtcNow;
            ExpiraEm = CriadoEm.AddDays(DiasDeValidade);
        }
    }
}
