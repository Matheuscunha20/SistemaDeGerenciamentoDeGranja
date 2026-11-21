// Arquivo: Program.cs (no projeto SistemaGranja.Web)

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using System;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // Usado pelo Pomelo para MySqlServerVersion
using SistemaGranja.Services;
using SistemaGranja.Web.Identidade;
using SistemaGranja.Web.Emails;
using Microsoft.AspNetCore.Identity.UI.Services;


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
.AddEntityFrameworkStores<AppDbContext>()
.AddErrorDescriber<MensagensIdentityPtBr>() // mensagens de erro do login em português
.AddClaimsPrincipalFactory<FabricaDeClaimsAgroArca>(); // guarda a fazenda do usuário no login

// Fazenda do usuário logado: usada pelo AppDbContext para separar os dados de cada cliente
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFazendaAtual, FazendaDoUsuarioLogado>();

// E-mails (confirmação, esqueci a senha, convite). Os dados do servidor ficam na seção "Email"
// (use dotnet user-secrets). Sem servidor configurado, em desenvolvimento os e-mails são salvos
// como arquivos .html na pasta EmailsDeTeste do projeto, para você abrir e clicar nos links.
builder.Services.Configure<ConfiguracaoEmail>(builder.Configuration.GetSection("Email"));
builder.Services.PostConfigure<ConfiguracaoEmail>(config =>
{
    if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(config.PastaDeTeste))
        config.PastaDeTeste = Path.Combine(builder.Environment.ContentRootPath, "EmailsDeTeste");
});
builder.Services.AddTransient<IEmailSender, EnviadorDeEmail>();
builder.Services.AddScoped<EmailsDaConta>();

// Perfis de acesso: quem pode fazer o quê
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Politicas.Gestao, p => p.RequireRole(nameof(PerfilUsuario.Dono), nameof(PerfilUsuario.Gerente)));
    options.AddPolicy(Politicas.Dono, p => p.RequireRole(nameof(PerfilUsuario.Dono)));
});

// A cada minuto o login é conferido com o banco: mudança de perfil vale logo
// e quem teve o acesso removido sai do sistema
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(1));
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
builder.Services.AddScoped<ITipoVacinaService, TipoVacinaService>();
builder.Services.AddScoped<IVacinacaoService, VacinacaoService>();
builder.Services.AddScoped<ISaudeService, SaudeService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IPainelService, PainelService>();
builder.Services.AddScoped<IEquipeService, EquipeService>();
builder.Services.AddScoped<IFazendaService, FazendaService>();

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

    // Espécies, raças e vacinas agora são de cada fazenda:
    // são cadastradas quando a conta (e a fazenda) é criada.
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

// Páginas de conta em inglês que ainda não usamos ficam fora do ar
app.Use(async (context, next) =>
{
    if (!PaginasDaConta.EstaLiberada(context.Request.Path.Value))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});

app.UseRouting();

app.UseAuthentication(); // Autenticação primeiro

// Quem entrou antes desta versão não tem a fazenda ou o perfil no login: pede para entrar de novo
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true
        && (context.User.FindFirst(FazendaDoUsuarioLogado.ClaimFazendaId) == null
            || context.User.FindFirst(System.Security.Claims.ClaimTypes.Role) == null)
        && !context.Request.Path.StartsWithSegments("/Identity"))
    {
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        context.Response.Redirect("/Identity/Account/Login");
        return;
    }
    await next();
});
app.UseAuthorization();  // Depois autorização

// Rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
