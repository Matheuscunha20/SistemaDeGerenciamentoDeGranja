// Localização CORRETA: Projeto SistemaGranja.Data / Arquivo: ApplicationUser.cs
using Microsoft.AspNetCore.Identity; // Pacote Microsoft.AspNetCore.Identity.EntityFrameworkCore deve estar no SistemaGranja.Data.csproj

namespace SistemaGranja.Data // Certifique-se de que o namespace é SistemaGranja.Data
{
    public class ApplicationUser : IdentityUser
    {
        // Você pode adicionar propriedades personalizadas ao usuário aqui no futuro, se precisar.
        // Exemplo: public string NomeCompleto { get; set; }
    }
}