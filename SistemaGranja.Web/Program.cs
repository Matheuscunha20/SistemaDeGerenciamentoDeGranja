// Arquivo: Program.cs (no projeto SistemaGranja.Web)

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using System;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
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
builder.Services.AddControllersWithViews(options =>
{
    // Só são obrigatórios os campos marcados com [Required] nas entidades
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

    // Mensagens de validação em português
    var mensagens = options.ModelBindingMessageProvider;
    mensagens.SetValueMustNotBeNullAccessor(campo => $"O campo {campo} é obrigatório.");
    mensagens.SetMissingBindRequiredValueAccessor(campo => $"O campo {campo} é obrigatório.");
    mensagens.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
    mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
    mensagens.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
    mensagens.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado para {campo} é inválido.");
});
builder.Services.AddRazorPages(); // Para Razor Pages e/ou UI do Identity
builder.Services.AddScoped<IGalpaoService, GalpaoService>();
builder.Services.AddScoped<ITipoAlimentoService, TipoAlimentoService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IEspecieService, EspecieService>();
builder.Services.AddScoped<IRacaService, RacaService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();

var app = builder.Build();

// Prepara o banco de dados ao iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Em desenvolvimento, cria o banco e aplica as migrations automaticamente
    if (app.Environment.IsDevelopment())
    {
        db.Database.Migrate();
    }

    // Cadastra espécies e raças comuns (só o que ainda não existe)
    await SemeadorDeDados.SemearEspeciesERacasAsync(db);
}

// Configure o pipeline de requisição HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Ou apenas /Error para Razor Pages
    app.UseHsts();
}

app.UseHttpsRedirection();

// Datas, números e moeda no padrão brasileiro (dd/MM/aaaa, vírgula decimal)
var culturaBrasil = new CultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaBrasil),
    SupportedCultures = new[] { culturaBrasil },
    SupportedUICultures = new[] { culturaBrasil }
});
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
