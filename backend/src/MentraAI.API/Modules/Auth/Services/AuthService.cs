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
using System.Net.Http;
using System.Net.Http.Json;
using Google.Apis.Auth;
using System.Text.Json.Serialization;

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
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
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

        return await BuildAuthResponseAsync(user);
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

        return await BuildAuthResponseAsync(user);
    }

    // Build shared AuthResponse
    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
    {
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

    // === External OAuth Logins ===

    // Google
    public async Task<AuthResponse> LoginWithGoogleAsync(string idToken)
    {
        var clientId = _config["Authentication:Google:ClientId"];
        var settings = new GoogleJsonWebSignature.ValidationSettings();
        if (!string.IsNullOrEmpty(clientId) && clientId != "YOUR_GOOGLE_CLIENT_ID")
        {
            settings.Audience = new[] { clientId };
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            var email = payload.Email.Trim().ToLowerInvariant();
            var firstName = payload.GivenName ?? string.Empty;
            var lastName = payload.FamilyName ?? string.Empty;
            var providerKey = payload.Subject;

            return await ProcessExternalLoginAsync(email, firstName, lastName, "Google", providerKey);
        }
        catch (Exception ex)
        {
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, $"Google token validation failed: {ex.Message}", 400);
        }
    }

    // GitHub
    public async Task<AuthResponse> LoginWithGitHubAsync(string code)
    {
        var clientId = _config["Authentication:GitHub:ClientId"];
        var clientSecret = _config["Authentication:GitHub:ClientSecret"];

        using var client = new HttpClient();
        
        // 1. Exchange code for access token
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
        tokenRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", clientId ?? string.Empty },
            { "client_secret", clientSecret ?? string.Empty },
            { "code", code }
        });

        var tokenResponse = await client.SendAsync(tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "Failed to exchange code with GitHub.", 400);

        var tokenContent = await tokenResponse.Content.ReadFromJsonAsync<GitHubTokenResponse>();
        if (tokenContent == null || string.IsNullOrEmpty(tokenContent.AccessToken))
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "GitHub did not return an access token.", 400);

        // 2. Fetch user profile
        var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        profileRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenContent.AccessToken);
        profileRequest.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("MentraAI", "1.0"));

        var profileResponse = await client.SendAsync(profileRequest);
        if (!profileResponse.IsSuccessStatusCode)
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "Failed to fetch user profile from GitHub.", 400);

        var gitHubUser = await profileResponse.Content.ReadFromJsonAsync<GitHubUserProfile>();
        if (gitHubUser == null)
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "GitHub profile parsing failed.", 400);

        var providerKey = gitHubUser.Id.ToString();
        var email = gitHubUser.Email;

        // 3. Fetch emails if email is null or private
        if (string.IsNullOrEmpty(email))
        {
            var emailsRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
            emailsRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenContent.AccessToken);
            emailsRequest.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("MentraAI", "1.0"));

            var emailsResponse = await client.SendAsync(emailsRequest);
            if (emailsResponse.IsSuccessStatusCode)
            {
                var emails = await emailsResponse.Content.ReadFromJsonAsync<List<GitHubEmail>>();
                var primaryEmail = emails?.FirstOrDefault(e => e.Primary && e.Verified)?.Email;
                email = primaryEmail ?? emails?.FirstOrDefault()?.Email;
            }
        }

        if (string.IsNullOrEmpty(email))
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "GitHub account must have a verified email.", 400);

        // Parse names
        var nameParts = (gitHubUser.Name ?? gitHubUser.Login).Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        return await ProcessExternalLoginAsync(email, firstName, lastName, "GitHub", providerKey);
    }

    // Apple
    public async Task<AuthResponse> LoginWithAppleAsync(string identityToken, string? firstName, string? lastName)
    {
        var clientId = _config["Authentication:Apple:ClientId"];
        
        using var client = new HttpClient();
        var jwksResponse = await client.GetAsync("https://appleid.apple.com/auth/keys");
        if (!jwksResponse.IsSuccessStatusCode)
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "Failed to retrieve Apple public keys.", 400);

        var jwksString = await jwksResponse.Content.ReadAsStringAsync();
        var keyset = new Microsoft.IdentityModel.Tokens.JsonWebKeySet(jwksString);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://appleid.apple.com",
            ValidateAudience = true,
            ValidAudience = clientId,
            IssuerSigningKeys = keyset.Keys,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(identityToken, validationParameters, out var validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;

            var providerKey = jwtToken.Subject;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            if (string.IsNullOrEmpty(email))
                throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "Apple Identity Token does not contain an email.", 400);

            return await ProcessExternalLoginAsync(email, firstName ?? string.Empty, lastName ?? string.Empty, "Apple", providerKey);
        }
        catch (Exception ex)
        {
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, $"Apple token validation failed: {ex.Message}", 400);
        }
    }

    // Helper to process external logins (link existing, or create new user + link)
    private async Task<AuthResponse> ProcessExternalLoginAsync(string email, string firstName, string lastName, string provider, string providerKey)
    {
        var user = await _userManager.FindByLoginAsync(provider, providerKey);
        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new AppException(ErrorCodes.VALIDATION_ERROR, $"Failed to create user: {errors}", 400);
                }

                _db.UserProfiles.Add(new UserProfile
                {
                    UserId = user.Id,
                    IsOnboarded = false,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider));
            if (!addLoginResult.Succeeded)
            {
                var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                throw new AppException(ErrorCodes.VALIDATION_ERROR, $"Failed to link {provider} login: {errors}", 400);
            }
        }

        if (!user.IsActive)
            throw new AppException(ErrorCodes.INVALID_CREDENTIALS, "User account is deactivated.", 401);

        return await BuildAuthResponseAsync(user);
    }

    // Nested private classes for GitHub API responses
    private class GitHubTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }

    private class GitHubUserProfile
    {
        public long Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    private class GitHubEmail
    {
        public string Email { get; set; } = string.Empty;
        public bool Primary { get; set; }
        public bool Verified { get; set; }
    }
}