using API;
using API.Jobs;
using API.Middlewares;
using API.Notificacoes.OS;
using Core.DTOs.Config;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using Datadog.Trace.Configuration;
using Hangfire;
using Hangfire.PostgreSql;
using Infraestrutura.Dados;
using Infraestrutura.Dados.Extensions;
using Infraestrutura.Logs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();
#endregion

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers().AddJsonOptions(options =>
             options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MecanicaOS API", Version = "v1" });

    // Adiciona suporte a autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Cabeçalho de autorização JWT usando o esquema Bearer. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Configuration.AddEnvironmentVariables();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<MecanicaContexto>(options =>
    options.UseNpgsql(connectionString, npgsqlOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    }));

// Configuração JWT
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<ConfiguracaoJwt>();
builder.Services.Configure<ConfiguracaoJwt>(builder.Configuration.GetSection("Jwt"));

// Autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Autorização
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpContextAccessor();

#region Datadog APM
var settings = TracerSettings.FromDefaultSources();
settings.AnalyticsEnabled = true;
Datadog.Trace.Tracer.Configure(settings);
#endregion

#region Infraestrutura
// Jobs Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

// Registrar Jobs
builder.Services.AddScoped<ICompositionRoot, CompositionRoot>();

builder.Services.AddTransient<VerificarEstoqueJob>();
builder.Services.AddTransient<VerificarOrcamentoExpiradoJob>();
builder.Services.AddTransient<RecurringJobs>();

// Notificações
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(OrdemServicoEmOrcamentoEvent).Assembly);
});

builder.Services.AddScoped<IdCorrelacionalLogMiddleware>();
builder.Services.AddScoped<IIdCorrelacionalService, IdCorrelacionalService>();

#endregion

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json" });
});

var app = builder.Build();

#region Middleware p/ rastrear e Logar requisições HTTP
app.Use(async (context, next) =>
{
    var span = Datadog.Trace.Tracer.Instance.ActiveScope?.Span;

    try
    {
        await next.Invoke();

        var statusCode = context.Response.StatusCode;
        span?.SetTag("http.status_code", statusCode.ToString());

        if (statusCode >= 500)
        {
            span.Error = true;
            Log.Error("Erro de Servidor: {Method} {Path} retornou {Status}",
                      context.Request.Method, context.Request.Path, statusCode);
        }
        else if (statusCode >= 400)
        {
            span?.SetTag("app.business_status", "warning");

            Log.Warning("Aviso de Negócio: {Method} {Path} retornou {Status}",
                        context.Request.Method, context.Request.Path, statusCode);
        }
        else
        {
            Log.Information("Sucesso: {Method} {Path} Status {Status}",
                            context.Request.Method, context.Request.Path, statusCode);
        }
    }
    catch (Exception ex)
    {
        span?.SetException(ex);
        Log.Error(ex, "Exceção Crítica na URL {Path}", context.Request.Path);
        throw;
    }
});
#endregion

app.UseResponseCompression();

app.UsePathBase(new PathString("/api/v1"));

app.UseRouting();

// Adiciona autenticação e autorização ao pipeline
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<IdCorrelacionalLogMiddleware>();

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        swaggerDoc.Servers = new List<OpenApiServer>
        {
            new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/api/v1" }
        };
    });
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/v1/swagger/v1/swagger.json", "MecanicaOS API v1");
    c.RoutePrefix = "docs";
});

app.UseReDoc(c =>
{
    c.DocumentTitle = "MecanicaOS API Documentation";
    c.SpecUrl = "/api/v1/swagger/v1/swagger.json";
    c.RoutePrefix = "api-docs";
});

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
});

#if DEBUG
app.UseHangfireDashboard("/hangfire");
#endif

using (var scope = app.Services.CreateScope())
{
    var recurringJobs = scope.ServiceProvider.GetRequiredService<RecurringJobs>();
    recurringJobs.ScheduleJobs();
}

try
{
    // Aplicar migrações automaticamente
    using var escopo = app.Services.CreateScope();
    app.Logger.LogInformation("Iniciando aplicação...");

    await app.Services.AplicarMigracoesAsync<MecanicaContexto>();

    app.Logger.LogInformation("Aplicação iniciada com sucesso!");
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Falha crítica ao iniciar a aplicação");
    throw;
}

app.Run();