using System.Threading.Tasks;

namespace SistemaGranja.Services
{
    public interface IPainelService
    {
        Task<ResumoPainel> ObterResumoAsync();
    }
}
