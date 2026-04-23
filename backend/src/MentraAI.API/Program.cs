using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MentraAI.API.Common.Middleware;
using MentraAI.API.Data;
using MentraAI.API.Modules.Auth.DTOs.Requests;
using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.Auth.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// === Controllers ==========================================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // FIX: Removed .Models namespace qualifiers
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MentraAI API", Version = "v1" });

    // Add JWT Authorization support to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // FIX: Wrapped in a delegate (document => ...)
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            // FIX: Replaced .Reference property with the new strongly-typed Reference class
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
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
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// === JWT ====
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
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

//  FluentValidation 
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

//  Module Services 
builder.Services.AddScoped<IAuthService, AuthService>();

//  Build App 
var app = builder.Build();

//  Middleware pipeline (ORDER IS CRITICAL) 
app.UseMiddleware<GlobalExceptionMiddleware>();   // MUST be first

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//  Auto-migrate on startup (development convenience only) 
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();