using MentraAI.API.Modules.Auth.DTOs.Requests;
using MentraAI.API.Modules.Auth.DTOs.Responses;

namespace MentraAI.API.Modules.Auth.Services;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken, string userId);
}