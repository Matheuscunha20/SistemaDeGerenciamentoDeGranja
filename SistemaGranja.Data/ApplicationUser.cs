using Microsoft.AspNetCore.Identity;
using SistemaGranja.Domain;

namespace SistemaGranja.Data
{
    public class ApplicationUser : IdentityUser
    {
        // Fazenda à qual o usuário pertence
        public int? FazendaId { get; set; }

        // O que o usuário pode fazer dentro da fazenda
        public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Dono;
    }
}
