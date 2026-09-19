using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TomerGroup.Api.Middleware;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Security;
using TomerGroup.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Dynamic Port Binding for Render Cloud Hosting
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// 1. Configure EF Core DbContext (PostgreSQL by default, with InMemory fallback for tests/offline)
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "PostgreSQL";
var defaultConnectionString = builder.Configuration["SUPABASE_DB_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=tomergroup_db;Username=postgres;Password=postgres";

builder.Services.AddDbContext<TomerDbContext>(options =>
{
    if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("TomerGroupInMemoryDb");
    }
    else
    {
        options.UseNpgsql(defaultConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        });
    }
});

// 2. Register Security & Cryptography services
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// 3. Register Business Domain Services & Supabase Storage
builder.Services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<ITransportationService, TransportationService>();
builder.Services.AddScoped<IGuideService, GuideService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IBrandingService, BrandingService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddSingleton<IRateLimitingService, RateLimitingService>();
builder.Services.AddSingleton<ISecuritySanitizerService, SecuritySanitizerService>();
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
builder.Services.AddSingleton<IMapService, MapService>();

// 4. Configure JWT Authentication (Supabase Auth + Internal Token Support)
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "TomerGroupSuperSecretKeyForPeruTravelExperience2026!@#$998877";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "TomerGroupApi";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "TomerGroupApp";
var supabaseUrl = builder.Configuration["SUPABASE_URL"] ?? builder.Configuration["SupabaseSettings:Url"];
var supabaseJwtSecret = builder.Configuration["SUPABASE_JWT_SECRET"] ?? builder.Configuration["SupabaseSettings:JwtSecret"];

var validIssuers = new List<string> { jwtIssuer };
if (!string.IsNullOrWhiteSpace(supabaseUrl))
{
    validIssuers.Add($"{supabaseUrl.TrimEnd('/')}/auth/v1");
}

var validAudiences = new List<string> { jwtAudience, "authenticated" };

var signingKeys = new List<SecurityKey>
{
    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
};

if (!string.IsNullOrWhiteSpace(supabaseJwtSecret) && supabaseJwtSecret != jwtSecret)
{
    signingKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseJwtSecret)));
}

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
        ValidIssuers = validIssuers,
        ValidateAudience = true,
        ValidAudiences = validAudiences,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKeys = signingKeys,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// 5. Configure Controllers & CORS
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 6. Configure Swagger with JWT Bearer scheme
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tomer Group - Peru Travel Experience API",
        Version = "v1",
        Description = "Enterprise travel platform API for Tomer Group (Cusco, Peru). Serving Israeli travelers with curated Andean itineraries."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// 7. Configure Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseTomerSecurityHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tomer Group API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 8. Auto-seed Database on Startup (handles PostgreSQL connectivity check gracefully)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<TomerDbContext>();
    var hasher = services.GetRequiredService<IPasswordHasher>();

    try
    {
        if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            context.Database.EnsureCreated();
            await DatabaseSeeder.SeedAsync(context, hasher, app.Environment.IsDevelopment());
            logger.LogInformation("In-Memory database created and seeded with Tomer Group Peru data (DevMode: {DevMode}).", app.Environment.IsDevelopment());
        }
        else
        {
            var canConnect = await context.Database.CanConnectAsync();
            if (canConnect)
            {
                context.Database.EnsureCreated();
                await DatabaseSeeder.SeedAsync(context, hasher, app.Environment.IsDevelopment());
                logger.LogInformation("PostgreSQL database successfully connected and initialized (DevMode: {DevMode}).", app.Environment.IsDevelopment());
            }
            else
            {
                logger.LogWarning("PostgreSQL database server at {Connection} is currently unavailable. API will operate in configuration-verified mode.", defaultConnectionString);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database auto-initialization deferred. API running with verified configuration.");
    }
}

app.Run();

// Needed for WebApplicationFactory in integration tests
public partial class Program { }
