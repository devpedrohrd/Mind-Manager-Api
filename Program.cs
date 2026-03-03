using Microsoft.EntityFrameworkCore;
using Mind_Manager.Infrastructure.Persistence;
using Mind_Manager.Api.Middlewares;
using Mind_Manager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Mind_Manager.src.Application.Services;
using System.Text.Json.Serialization;
using Mind_Manager.Domain.Validators;
using Mind_Manager.Application.Authorization;
using Mind_Manager.src.Domain.Interfaces;
using Mind_Manager.src.Infrastructure.Repository;
using ISession = Mind_Manager.src.Domain.Interfaces.ISession;
using Microsoft.AspNetCore.HttpOverrides;

// Carrega variáveis do arquivo .env (desenvolvimento local)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load();
    Console.WriteLine("[ENV] Variáveis carregadas do arquivo .env");
}

var builder = WebApplication.CreateBuilder(args);

// Variáveis de ambiente sobrescrevem appsettings.json
builder.Configuration.AddEnvironmentVariables();

// ========================================
// CONFIGURAÇÃO 1: SERVICES (Injeção de Dependência)
// ========================================

// Controllers e API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {        {
            new OpenApiSecurityScheme
            {                Reference = new OpenApiReference
                {                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }    });
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Mind Manager API",
        Description = "API para gerenciamento de consultas, pacientes e psicólogos.",
        Contact = new OpenApiContact
        {
            Name = "Equipe Mind Manager",
        }
    });

    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"))
            )
        };

        // Adiciona eventos para debug de autenticação
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("JWT Authentication failed: {Exception}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
                logger.LogInformation("JWT Token validated successfully. UserId: {UserId}, Role: {Role}", userId, role);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT Challenge triggered. Error: {Error}, ErrorDescription: {ErrorDescription}",
                    context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
     .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
     .AddPolicy("PsychologistOrAdmin", policy => policy.RequireRole("Admin", "Psychologist"))
     .AddPolicy("AllUsers", policy => policy.RequireRole("Admin", "Psychologist", "Client"));


// ========================================
// CONFIGURAÇÃO 2: DATABASE CONTEXT
// ========================================
// Adiciona DbContext com string de conexão do appsettings.json
// String de conexão: appsettings.json → ConnectionStrings → DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        ApplicationDbContext.ConfigureEnums(npgsqlOptions);
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }).EnableServiceProviderCaching(false));


// ========================================
// CONFIGURAÇÃO 3: DEPENDENCY INJECTION (DI Container)
// ========================================

builder.Services.AddScoped<Mind_Manager.Domain.Interfaces.IUnitOfWork, Mind_Manager.Infrastructure.UnitOfWork.UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointment, AppointmentRepository>();
builder.Services.AddScoped<IPsychologist, PsychologistRepository>();
builder.Services.AddScoped<IPatient, PatientRepository>();
builder.Services.AddScoped<ISession, SessionRepository>();
builder.Services.AddScoped<IAnamnesis, AnamnesisRepository>();
builder.Services.AddScoped<IEmailSchedule, EmailScheduleRepository>();
builder.Services.AddScoped<IUserValidator, UserValidator>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPsychologistService, PsychologistService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IAnamneseService, AnamneseService>();

// Background Service: Lembrete de email diário às 6h
builder.Services.AddHostedService<EmailReminderBackgroundService>();

builder.Services.AddSingleton<Mind_Manager.Api.Middlewares.ExceptionHandlers.IExceptionHandler, Mind_Manager.Api.Middlewares.ExceptionHandlers.NotFoundExceptionHandler>();
builder.Services.AddSingleton<Mind_Manager.Api.Middlewares.ExceptionHandlers.IExceptionHandler, Mind_Manager.Api.Middlewares.ExceptionHandlers.BusinessExceptionHandler>();
builder.Services.AddSingleton<Mind_Manager.Api.Middlewares.ExceptionHandlers.IExceptionHandler, Mind_Manager.Api.Middlewares.ExceptionHandlers.ValidationExceptionHandler>();
builder.Services.AddSingleton<Mind_Manager.Api.Middlewares.ExceptionHandlers.IExceptionHandler, Mind_Manager.Api.Middlewares.ExceptionHandlers.ForbiddenExceptionHandler>();
builder.Services.AddSingleton<Mind_Manager.Api.Middlewares.ExceptionHandlers.IExceptionHandler, Mind_Manager.Api.Middlewares.ExceptionHandlers.UnauthorizedAccessExceptionHandler>();
builder.Services.AddSingleton<Mind_Manager.Api.Middlewares.ExceptionHandlers.IExceptionHandler, Mind_Manager.Api.Middlewares.ExceptionHandlers.ArgumentExceptionHandler>();
builder.Services.AddSingleton<Mind_Manager.Api.Middlewares.ExceptionHandlers.IExceptionHandler, Mind_Manager.Api.Middlewares.ExceptionHandlers.GenericExceptionHandler>();
builder.Services.AddSingleton<IUserLoggedHandller, UserLoggedHandller>();

// Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// CORS (para aceitar requisições de outros domínios)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ========================================
// CONSTRUIR APP
// ========================================
var app = builder.Build();
app.UseForwardedHeaders();

// ========================================
// MIDDLEWARE PIPELINE
// ========================================
// Ordem importa! Middlewares executam nesta ordem.

// Middleware de tratamento de exceções global
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "MindManager API is running 🚀");

// CORS
app.UseCors("AllowAll");

// Roteamento
app.UseRouting();

// Autenticação e Autorização (DEVE ficar entre UseRouting e MapControllers)
app.UseAuthentication();
app.UseAuthorization();

// Mapear controllers
app.MapControllers();

// ========================================
// MIGRATION AUTOMÁTICA (DESENVOLVIMENTO)
// ========================================
// Em desenvolvimento, cria/atualiza banco automaticamente
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // dbContext.Database.EnsureDeleted(); // Descomentar para resetar BD
        // dbContext.Database.Migrate(); // Executa migrations pendentes
    }
}

// ========================================
// RUN
// ========================================
app.Run();
