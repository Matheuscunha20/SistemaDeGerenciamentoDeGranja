using SistemaGranja.Data;

namespace SistemaGranja.Web.Identidade
{
    // Lê a fazenda do usuário logado a partir do login (claim "FazendaId")
    public class FazendaDoUsuarioLogado : IFazendaAtual
    {
        public const string ClaimFazendaId = "FazendaId";
        public const string ClaimFazendaNome = "FazendaNome";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public FazendaDoUsuarioLogado(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? FazendaId
        {
            get
            {
                var valor = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimFazendaId)?.Value;
                return int.TryParse(valor, out var id) ? id : null;
            }
        }
    }
}
