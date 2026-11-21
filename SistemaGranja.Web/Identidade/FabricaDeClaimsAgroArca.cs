using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SistemaGranja.Data;

namespace SistemaGranja.Web.Identidade
{
    // Ao fazer login, guarda no cookie a fazenda do usuário (Id e nome)
    public class FabricaDeClaimsAgroArca : UserClaimsPrincipalFactory<ApplicationUser>
    {
        private readonly AppDbContext _db;

        public FabricaDeClaimsAgroArca(
            UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor,
            AppDbContext db)
            : base(userManager, optionsAccessor)
        {
            _db = db;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identidade = await base.GenerateClaimsAsync(user);

            if (user.FazendaId.HasValue)
            {
                var fazenda = await _db.Fazendas.FindAsync(user.FazendaId.Value);
                identidade.AddClaim(new Claim(FazendaDoUsuarioLogado.ClaimFazendaId, user.FazendaId.Value.ToString()));
                identidade.AddClaim(new Claim(FazendaDoUsuarioLogado.ClaimFazendaNome, fazenda?.Nome ?? string.Empty));
            }

            return identidade;
        }
    }
}
