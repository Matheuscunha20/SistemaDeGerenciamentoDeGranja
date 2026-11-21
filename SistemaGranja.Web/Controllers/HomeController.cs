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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
