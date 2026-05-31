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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === Controllers ===
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

builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizScoringService, QuizScoringService>();
builder.Services.AddScoped<IQuizService, QuizService>();

builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

// === Build App ===
var app = builder.Build();

// === Middleware pipeline ===
app.UseMiddleware<GlobalExceptionMiddleware>();

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
app.Run();