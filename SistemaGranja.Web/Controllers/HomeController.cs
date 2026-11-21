using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SistemaGranja.Services;
using SistemaGranja.Web.Models;

namespace SistemaGranja.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPainelService _painelService;

        public HomeController(ILogger<HomeController> logger, IPainelService painelService)
        {
            _logger = logger;
            _painelService = painelService;
        }

        // Logado: painel com números e alertas. Visitante: página de apresentação.
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated != true)
                return View();

            return View("Painel", await _painelService.ObterResumoAsync());
        }

        // Erro inesperado: página amigável. O código ajuda a achar o erro no log.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Páginas de erro por código (404 não encontrada, 403 sem permissão...)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Status(int codigo)
        {
            Response.StatusCode = codigo is >= 400 and <= 599 ? codigo : 404;
            return View(Response.StatusCode);
        }
    }
}
