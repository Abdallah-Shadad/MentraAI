using FluentValidation;
using MentraAI.API.Common.Middleware;
using MentraAI.API.Data;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.Auth.DTOs.Requests;
using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.Auth.Services;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.CareerTracks.Services;
using MentraAI.API.Modules.Chat.Repositories;
using MentraAI.API.Modules.Chat.Services;
using MentraAI.API.Modules.Onboarding.Repositories;
using MentraAI.API.Modules.Onboarding.Services;
using MentraAI.API.Modules.Quizzes.Repositories;
using MentraAI.API.Modules.Quizzes.Services;
using MentraAI.API.Modules.Roadmaps.Repositories;
using MentraAI.API.Modules.Roadmaps.Services;
using MentraAI.API.Modules.StageProgress.Repositories;
using MentraAI.API.Modules.StageProgress.Services;
using MentraAI.API.Modules.Users.Repositories;
using MentraAI.API.Modules.Users.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === Controllers ===
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // Collect all validation errors in a structured way
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            // Formulate the response in the agreed standard
            var errorResponse = new
            {
                success = false,
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "Validation failed.",
                    statusCode = 400,
                    errors = errors
                }
            };

            return new BadRequestObjectResult(errorResponse);
        };
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MentraAI API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Enter: Bearer {your token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// === Database ===

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));


//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        sql => sql.EnableRetryOnFailure(
//            maxRetryCount: 3,
//            maxRetryDelay: TimeSpan.FromSeconds(5),
//            errorNumbersToAdd: null)));

// === Identity ===
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// === AIGateway ===
builder.Services
    .AddHttpClient<IAIGatewayService, AIGatewayService>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["AIService:BaseUrl"]!);
        client.Timeout = TimeSpan.FromSeconds(350);
    })
   .AddStandardResilienceHandler(options =>
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(300);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(600);
        options.Retry.MaxRetryAttempts = 1;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.Retry.UseJitter = true;
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(350);
    });
// === JWT ===
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("access_token", out var accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());

    options.AddPolicy("ProdPolicy", policy =>
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// === AutoMapper ===
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(Program));
});
// === FluentValidation ===
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// === Module Services ===
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IOnboardingRepository, OnboardingRepository>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();

builder.Services.AddScoped<ICareerTrackRepository, CareerTrackRepository>();
builder.Services.AddScoped<ICareerTrackService, CareerTrackService>();

builder.Services.AddScoped<IRoadmapRepository, RoadmapRepository>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();

builder.Services.AddScoped<IStageProgressRepository, StageProgressRepository>();
builder.Services.AddScoped<IStageProgressService, StageProgressService>();

builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizScoringService, QuizScoringService>();
builder.Services.AddScoped<IQuizService, QuizService>();

builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

// === Configure PORT for cloud hosting (Render) ===
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// === Build App ===
var app = builder.Build();

// === Middleware pipeline ===
app.UseMiddleware<GlobalExceptionMiddleware>();

// Track HTTP request metrics (duration, count, in-flight) for Prometheus
// Must be placed early in the pipeline to capture all requests
app.UseHttpMetrics();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MentraAI API v1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
    app.UseCors("DevPolicy");
else
    app.UseCors("ProdPolicy");

app.UseAuthentication();
app.UseAuthorization();

// === Migrations ===
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
try
{
    db.Database.Migrate();
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Migration failed on startup.");
}

app.MapControllers();

// Expose Prometheus metrics at /metrics
// Prometheus scrapes this endpoint to collect request metrics
app.MapMetrics();

app.Run();