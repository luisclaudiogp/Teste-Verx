using Lancamentos.Application.Services;
using Lancamentos.Domain.Repositories;
using Lancamentos.Infrastructure.Data;
using Lancamentos.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Segurança (JWT / Keycloak)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

// 2. Observabilidade (OpenTelemetry)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("Lancamentos.API")
        .AddSource("MassTransit")
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Lancamentos.API"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter(opt => opt.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!)))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

// 3. Resiliência (Health Checks)
var rabbitHost = builder.Configuration["RabbitMQ:Host"];
var rabbitUser = builder.Configuration["RabbitMQ:Username"];
var rabbitPass = builder.Configuration["RabbitMQ:Password"];

builder.Services.AddSingleton<RabbitMQ.Client.IConnectionFactory>(sp =>
{
    return new RabbitMQ.Client.ConnectionFactory
    {
        Uri = new Uri($"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}")
    };
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRabbitMQ();

// Configuração do DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LancamentosDbContext>(options =>
    options.UseNpgsql(connectionString));

// Injeção de Dependência - Camadas
builder.Services.AddScoped<ILancamentoRepository, LancamentoRepository>();
builder.Services.AddScoped<ILancamentoService, LancamentoService>();

// MassTransit com Outbox
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<LancamentosDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbitConfig["Host"], "/", h =>
        {
            h.Username(rabbitConfig["Username"]!);
            h.Password(rabbitConfig["Password"]!);
        });
    });
});

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira o token JWT."
        };

        document.Components ??= new OpenApiComponents();
        if (document.Components.SecuritySchemes == null)
        {
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
        }
        document.Components.SecuritySchemes.Add("Bearer", scheme);

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [ new OpenApiSecuritySchemeReference("Bearer", document) ] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Middlewares
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

// Endpoints de Resiliência e Monitoramento
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Resiliência na Inicialização do Banco
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LancamentosDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    int retries = 5;
    while (retries > 0)
    {
        try
        {
            context.Database.EnsureCreated();
            break;
        }
        catch (Exception)
        {
            retries--;
            if (retries == 0) throw;
            logger.LogWarning("Aguardando banco de dados... {Retries} tentativas restantes", retries);
            Thread.Sleep(5000);
        }
    }
}

app.MapControllers();
app.Run();
