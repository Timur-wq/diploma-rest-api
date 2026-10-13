using Diploma.Application.Services;
using Diploma.Core.Abstractions;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Diploma.Core.Settings;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Diploma.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

//Загрузка настроек JWT с проверкой
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<Diploma.Application.Services.JwtSettings>();

if (string.IsNullOrEmpty(jwtSettings?.SecretKey) || jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters!");
}

var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

//Аутентификация
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
});

//Авторизация с политиками
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DispatcherOnly", policy => policy.RequireRole("Dispatcher"));
    options.AddPolicy("AccountantOnly", policy => policy.RequireRole("Accountant"));
    options.AddPolicy("OmtOnly", policy => policy.RequireRole("OmtEmployee"));
    options.AddPolicy("EngineerOnly", policy => policy.RequireRole("ServiceEngineer"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

//JSON настройки
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

//Swagger с JWT
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Diploma API", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите: Bearer {ваш_токен}"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

//Логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// CORS - поддержка веб-клиента и Flutter Web
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAllClients", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;

                var uri = new Uri(origin);

                // localhost или 127.0.0.1
                if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                {
                    // Веб-клиент (React/Vue/Angular)
                    if (uri.Port == 3000 || uri.Port == 5173 || uri.Port == 4200)
                        return true;

                    // Flutter Web (порты 50000+)
                    if (uri.Port >= 50000)
                        return true;

                    // Другие порты разработки
                    if (uri.Port >= 30000 && uri.Port < 50000)
                        return true;
                }

                return false; // Продакшен домены добавьте здесь
            })
            .AllowAnyMethod()
            .AllowAnyHeader();
        // Без AllowCredentials()!
    });
});

//DbContext
builder.Services.AddDbContext<DiplomaDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("db"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("Diploma.API");
        }));

//Регистрация сервисов
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();

builder.Services.AddSingleton<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();

// Регистрация сервиса сотрудников
builder.Services.AddScoped<IStaffService, StaffService>();

builder.Services.AddScoped<IRequestAdminService, RequestAdminService>();
builder.Services.AddScoped<IRequestAdminRepository, RequestAdminRepository>();

//Регистрация сервисов авторизации
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();

builder.Services.AddScoped<IEngineerTaskRepository, EngineerTaskRepository>();
builder.Services.AddScoped<IEngineerTaskService, EngineerTaskService>();

builder.Services.AddScoped<IDiagnosticActService, DiagnosticActService>();

builder.Services.AddScoped<IProcurementOptimizationService, ProcurementOptimizationService>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
// 🔥 Регистрация вашего нового сервиса
builder.Services.AddScoped<IReportService, ReportService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
}

//Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // Разрешает доступ к статическим файлам из wwwroot

app.UseHttpsRedirection();
app.UseCors("AllowAllClients");
app.UseAuthentication();  
app.UseAuthorization();   

app.MapControllers();
app.Run();