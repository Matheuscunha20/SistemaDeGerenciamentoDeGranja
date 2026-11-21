// Arquivo: Program.cs (no projeto SistemaGranja.Web)
// Configura e inicia o AgroArca. As configurações de produção vêm de variáveis de ambiente
// (veja a seção "Colocar no ar" do README).

using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using SistemaGranja.Data;
using SistemaGranja.Domain;
using SistemaGranja.Services;
using SistemaGranja.Web.Emails;
using SistemaGranja.Web.Identidade;
using SistemaGranja.Web.Producao;

var builder = WebApplication.CreateBuilder(args);

// ---------- Registro (logs) ----------
// Uma linha por evento, com data e hora: fácil de ler no painel da hospedagem
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(opcoes =>
{
    opcoes.SingleLine = true;
    opcoes.TimestampFormat = "dd/MM/yyyy HH:mm:ss ";
});

// ---------- Banco de dados ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta a conexão com o banco. Configure ConnectionStrings:DefaultConnection " +
        "(em desenvolvimento: dotnet user-secrets; no servidor: variável ConnectionStrings__DefaultConnection).");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 27))));

// ---------- Login (ASP.NET Core Identity) ----------
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddErrorDescriber<MensagensIdentityPtBr>() // mensagens de erro do login em português
.AddClaimsPrincipalFactory<FabricaDeClaimsAgroArca>(); // guarda a fazenda do usuário no login

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AgroArca.Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // No servidor o cookie de login só trafega por HTTPS
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// Chaves que protegem cookies e links de e-mail. No servidor ficam guardadas no banco, para não se
// perderem quando o sistema reinicia ou é atualizado (senão todos seriam desconectados).
// DataProtection:Pasta, se configurada, guarda numa pasta em vez do banco.
var protecaoDeDados = builder.Services.AddDataProtection().SetApplicationName("AgroArca");
var pastaDasChaves = builder.Configuration["DataProtection:Pasta"];
if (!string.IsNullOrWhiteSpace(pastaDasChaves))
    protecaoDeDados.PersistKeysToFileSystem(new DirectoryInfo(pastaDasChaves));
else if (!builder.Environment.IsDevelopment())
    protecaoDeDados.PersistKeysToDbContext<AppDbContext>();

// Fazenda do usuário logado: usada pelo AppDbContext para separar os dados de cada cliente
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFazendaAtual, FazendaDoUsuarioLogado>();

// ---------- E-mails ----------
// Dados do servidor na seção "Email" (user-secrets ou variáveis Email__Servidor, Email__Usuario...).
// Sem servidor configurado, em desenvolvimento os e-mails são salvos na pasta EmailsDeTeste.
builder.Services.Configure<ConfiguracaoEmail>(builder.Configuration.GetSection("Email"));
builder.Services.PostConfigure<ConfiguracaoEmail>(config =>
{
    if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(config.PastaDeTeste))
        config.PastaDeTeste = Path.Combine(builder.Environment.ContentRootPath, "EmailsDeTeste");
});
builder.Services.AddTransient<IEmailSender, EnviadorDeEmail>();
builder.Services.AddScoped<EmailsDaConta>();

// ---------- Perfis de acesso ----------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Politicas.Gestao, p => p.RequireRole(nameof(PerfilUsuario.Dono), nameof(PerfilUsuario.Gerente)));
    options.AddPolicy(Politicas.Dono, p => p.RequireRole(nameof(PerfilUsuario.Dono)));
});

// A cada minuto o login é conferido com o banco: mudança de perfil vale logo
// e quem teve o acesso removido sai do sistema
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(1));

// ---------- Produção ----------
// Saúde do sistema em /saude (usado pela hospedagem e pelo monitoramento)
builder.Services.AddHealthChecks().AddCheck<VerificacaoDoBanco>("banco");

// Limite de tentativas no login, cadastro e esqueci a senha
builder.Services.AddRateLimiter(LimiteDeTentativas.Configurar);

// Atrás de um proxy (Nginx, Caddy, plataforma de hospedagem) que cuida do HTTPS:
// confia nos cabeçalhos X-Forwarded-* para saber o IP real e que o acesso foi por HTTPS
var atrasDeProxy = builder.Configuration.GetValue<bool>("Proxy:ConfiarEmCabecalhos");
if (atrasDeProxy)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// ---------- Telas ----------
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
builder.Services.AddRazorPages(); // Telas de conta (login, cadastro...)

// ---------- Serviços do AgroArca ----------
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
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddScoped<ISaidaDeAnimaisService, SaidaDeAnimaisService>();

var app = builder.Build();
var logger = app.Logger;

// ---------- Ao iniciar ----------
// Atualiza o banco (migrations). Pode ser desligado com Banco__AplicarMigracoesAoIniciar=false.
// No computador desiste rápido (o banco do Docker já deveria estar ligado);
// no servidor espera até 1 minuto, porque o MySQL pode estar subindo junto.
if (app.Configuration.GetValue("Banco:AplicarMigracoesAoIniciar", true))
{
    if (app.Environment.IsDevelopment())
        await MigracoesAoIniciar.AplicarAsync(app.Services, logger, tentativas: 2, segundosEntreTentativas: 2);
    else
        await MigracoesAoIniciar.AplicarAsync(app.Services, logger);
}

if (!app.Environment.IsDevelopment())
{
    if (!app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<ConfiguracaoEmail>>().Value.Configurado)
        logger.LogWarning("Servidor de e-mail não configurado: confirmação de cadastro, esqueci a senha e convites não serão enviados.");
}

// ---------- Caminho de cada requisição ----------
if (atrasDeProxy)
    app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // erro inesperado: página amigável (o detalhe vai para o log)
    app.UseHsts();
}

// Páginas de erro em português (404 página não encontrada, 403...)
app.UseStatusCodePagesWithReExecute("/Home/Status", "?codigo={0}");

app.UseCabecalhosDeSeguranca();
app.UseHttpsRedirection();

// Datas, números e moeda no padrão brasileiro (dd/MM/aaaa, vírgula decimal)
var culturaBrasil = new CultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaBrasil),
    SupportedCultures = new[] { culturaBrasil },
    SupportedUICultures = new[] { culturaBrasil }
});
app.UseStaticFiles(new StaticFileOptions
{
    // O service worker (app instalável) precisa ser conferido a cada visita, para as atualizações chegarem
    OnPrepareResponse = contexto =>
    {
        if (contexto.File.Name == "sw.js")
            contexto.Context.Response.Headers.CacheControl = "no-cache";
    }
});

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
app.UseRateLimiter();

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

// Saúde do sistema: responde 200 (tudo certo) ou 503 (com problema). Não mostra detalhes internos.
var versao = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
app.MapHealthChecks("/saude", new HealthCheckOptions
{
    ResponseWriter = async (context, relatorio) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = relatorio.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy ? "ok" : "problema",
            versao,
            verificacoes = relatorio.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString())
        }));
    }
});

// Rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
