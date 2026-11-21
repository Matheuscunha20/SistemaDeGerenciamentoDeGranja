// Arquivo: Program.cs (no projeto SistemaGranja.Web)

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using System;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // Usado pelo Pomelo para MySqlServerVersion
using SistemaGranja.Services;


var builder = WebApplication.CreateBuilder(args);

// --- INÍCIO DAS SUAS ADIÇÕES DE SERVIÇO ---
// 1. Pegar a string de conexão
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Configurar o AppDbContext com Pomelo
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 27)))); // Ajuste para a versão real do seu MySQL, se necessário

// 3. Configurar o ASP.NET Core Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>();
// --- FIM DAS SUAS ADIÇÕES DE SERVIÇO ---

// Serviços padrão
builder.Services.AddControllersWithViews(); // Para MVC
builder.Services.AddRazorPages(); // Para Razor Pages e/ou UI do Identity
builder.Services.AddScoped<IGalpaoService, GalpaoService>();
builder.Services.AddScoped<ITipoAlimentoService, TipoAlimentoService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();

var app = builder.Build();

// Em desenvolvimento, cria o banco e aplica as migrations automaticamente
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure o pipeline de requisição HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Ou apenas /Error para Razor Pages
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Autenticação primeiro
app.UseAuthorization();  // Depois autorização

// Rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
