using Microsoft.AspNetCore.Identity;

namespace SistemaGranja.Data
{
    public class ApplicationUser : IdentityUser
    {
        // Fazenda à qual o usuário pertence
        public int? FazendaId { get; set; }
    }
}
