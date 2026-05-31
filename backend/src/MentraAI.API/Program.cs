using FluentValidation;
using MentraAI.API.Common.Middleware;
using MentraAI.API.Data;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.Auth.DTOs.Requests;
using MentraAI.API.Modules.Auth.Mappings;
using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.Auth.Services;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.CareerTracks.Services;
using MentraAI.API.Modules.Onboarding.Repositories;
using MentraAI.API.Modules.Onboarding.Services;
using MentraAI.API.Modules.Users.Mappings;
using MentraAI.API.Modules.Users.Repositories;
using MentraAI.API.Modules.Users.Services;
using MentraAI.API.Modules.Roadmaps.Repositories;
using MentraAI.API.Modules.Roadmaps.Services;
using MentraAI.API.Modules.StageProgress.Repositories;
using MentraAI.API.Modules.StageProgress.Services;
using MentraAI.API.Modules.Chat.Repositories;
using MentraAI.API.Modules.Chat.Services;
using MentraAI.API.Modules.Quizzes.Repositories;
using MentraAI.API.Modules.Quizzes.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === Controllers ====
builder.Services.AddControllers();
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
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// === Identity ===
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false; // No special character requirement for better UX, but can be enabled if desired

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// == AIGateway Module ==
builder.Services
    .AddHttpClient<IAIGatewayService, AIGatewayService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["AIService:BaseUrl"]!);
        client.Timeout = TimeSpan.FromSeconds(130);
    });
//.AddStandardResilienceHandler(options =>
//{
//    // Retry 3 times on transient failures (503, 502, 500, timeout)
//    options.Retry.MaxRetryAttempts = 3;
//    options.Retry.Delay = TimeSpan.FromSeconds(2);
//    options.Retry.BackoffType = DelayBackoffType.Exponential;
//    // Per-attempt timeout — AI is slow (multi-agent pipeline)
//    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(120);
//}
//);

// === JWT ====
builder.Services
    .AddHttpClient<IAIGatewayService, AIGatewayService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["AIService:BaseUrl"]!);
        client.Timeout = TimeSpan.FromSeconds(130);
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
    });

builder.Services.AddAuthorization();

// === CORS ===
builder.Services.AddCors(options =>
{
    // development: allow all origins (for ease of testing with various frontends)
    options.AddPolicy("DevPolicy", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());

    // production: restrict to allowed origins
    options.AddPolicy("ProdPolicy", policy =>
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});




//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("Frontend", policy =>
//        policy.WithOrigins(
//                builder.Configuration["Cors:AllowedOrigins"]!
//                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials());
//});

//  AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

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

// == Quizzes Module ==
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizScoringService, QuizScoringService>();
builder.Services.AddScoped<IQuizService, QuizService>();

// == Chat Module ==
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

//  Build App 
var app = builder.Build();

// === Middleware pipeline ===
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MentraAI API v1");
    c.RoutePrefix = "swagger";
});

// NEW — Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MentraAI API v1");
    c.RoutePrefix = "swagger";
});

// HTTPS redirection (optional, but recommended in production)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS must be before auth/authorization and after exception handling
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevPolicy"); // allow all origins in development for ease of testing with various frontends  
}
else
{
    app.UseCors("ProdPolicy"); // restrict to allowed origins in production for security  
}
app.UseAuthentication();
app.UseAuthorization();

// NEW — runs in all environments including production
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
app.Run();