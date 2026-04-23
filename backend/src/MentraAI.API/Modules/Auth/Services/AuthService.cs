using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Data;
using MentraAI.API.Modules.Auth.DTOs.Requests;
using MentraAI.API.Modules.Auth.DTOs.Responses;
using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.Users.Models;
using AutoMapper;

namespace MentraAI.API.Modules.Auth.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IConfiguration config,
        IMapper mapper)
    {
        _userManager = userManager;
        _db = db;
        _config = config;
        _mapper = mapper;
    }

    // Register
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
            throw new AppException(ErrorCodes.EMAIL_ALREADY_EXISTS, "Email is already registered.", 409);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new AppException(ErrorCodes.VALIDATION_ERROR, errors, 400);
        }

        // Create UserProfile row
        _db.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            IsOnboarded = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return _mapper.Map<RegisterResponse>(user);
    }

    // Login
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "Invalid email or password.", 401);

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "Invalid email or password.", 401);

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        var accessToken = GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id,
            expiryDays: int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!));

        var userSummary = _mapper.Map<UserSummary>(user);
        userSummary.IsOnboarded = profile?.IsOnboarded ?? false;
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!) * 60,

            User = userSummary
        };
    }

    // Refresh Token
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenRecord = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken
                                   && !t.IsRevoked
                                   && t.ExpiresAt > DateTime.UtcNow);

        if (tokenRecord is null)
            throw new AppException(ErrorCodes.REFRESH_TOKEN_INVALID,
                "Invalid or expired refresh token.", 401);

        // Revoke old token
        tokenRecord.IsRevoked = true;
        _db.RefreshTokens.Update(tokenRecord);
        await _db.SaveChangesAsync();

        var newAccessToken = GenerateAccessToken(tokenRecord.User);
        var newRefreshToken = await CreateRefreshTokenAsync(tokenRecord.UserId,
            expiryDays: int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!));

        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!) * 60
        };
    }

    // Logout
    public async Task LogoutAsync(string refreshToken, string userId)
    {
        var tokenRecord = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken && t.UserId == userId);

        if (tokenRecord is not null)
        {
            tokenRecord.IsRevoked = true;
            _db.RefreshTokens.Update(tokenRecord);
            await _db.SaveChangesAsync();
        }
    }

    // === Generate Access Token ===
    private string GenerateAccessToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email,          user.Email!),
            new Claim("firstName",               user.FirstName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // === Create Refresh Token ===
    private async Task<string> CreateRefreshTokenAsync(string userId, int expiryDays)
    {
        var token = new RefreshToken
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();

        return token.Token;
    }
}