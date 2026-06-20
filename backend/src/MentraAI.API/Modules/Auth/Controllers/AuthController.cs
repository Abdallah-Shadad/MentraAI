using FluentValidation;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Common.Models;
using MentraAI.API.Modules.Auth.DTOs.Requests;
using MentraAI.API.Modules.Auth.DTOs.Responses;
using MentraAI.API.Modules.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MentraAI.API.Modules.Auth.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;
    private readonly IValidator<LogoutRequest> _logoutValidator;
    private readonly IValidator<GoogleLoginRequest> _googleLoginValidator;
    private readonly IValidator<GitHubLoginRequest> _githubLoginValidator;
    private readonly IValidator<AppleLoginRequest> _appleLoginValidator;

    public AuthController(
        IAuthService authService,
        IWebHostEnvironment env,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshValidator,
        IValidator<LogoutRequest> logoutValidator,
        IValidator<GoogleLoginRequest> googleLoginValidator,
        IValidator<GitHubLoginRequest> githubLoginValidator,
        IValidator<AppleLoginRequest> appleLoginValidator)
    {
        _authService = authService;
        _env = env;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _logoutValidator = logoutValidator;
        _googleLoginValidator = googleLoginValidator;
        _githubLoginValidator = githubLoginValidator;
        _appleLoginValidator = appleLoginValidator;
    }

    // == POST /api/v1/auth/register ===
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new AppException(ErrorCodes.VALIDATION_ERROR, "Validation failed.", 400, errors);
        }

        var result = await _authService.RegisterAsync(request);
        SetTokenCookies(result.AccessToken, result.RefreshToken, result.ExpiresIn);
        return StatusCode(201, ApiResponse<AuthResponse>.Ok(result));
    }

    // == POST /api/v1/auth/login ====
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            throw new AppException(ErrorCodes.VALIDATION_ERROR, "Validation failed.", 400);
        }

        var result = await _authService.LoginAsync(request);
        SetTokenCookies(result.AccessToken, result.RefreshToken, result.ExpiresIn);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    // == POST /api/v1/auth/refresh ====
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest? request)
    {
        request ??= new RefreshTokenRequest();
        if (string.IsNullOrWhiteSpace(request.RefreshToken) && Request.Cookies.TryGetValue("refresh_token", out var cookieToken))
        {
            request.RefreshToken = cookieToken;
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Unauthorized(ApiResponse<object>.Fail(ErrorCodes.UNAUTHORIZED, "Missing refresh cookie", 401));
        }

        var validation = await _refreshValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            throw new AppException(ErrorCodes.VALIDATION_ERROR, "Refresh token is required.", 400);
        }

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken, result.ExpiresIn);
        return Ok(ApiResponse<TokenResponse>.Ok(result));
    }

    // == POST /api/v1/auth/logout ====
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        request ??= new LogoutRequest();
        if (string.IsNullOrWhiteSpace(request.RefreshToken) && Request.Cookies.TryGetValue("refresh_token", out var cookieToken))
        {
            request.RefreshToken = cookieToken;
        }

        var validation = await _logoutValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            throw new AppException(ErrorCodes.VALIDATION_ERROR, "Refresh token is required.", 400);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _authService.LogoutAsync(request.RefreshToken, userId);
        ClearTokenCookies();
        return NoContent();
    }

    // == POST /api/v1/auth/google ===
    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequest request)
    {
        var validation = await _googleLoginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            throw new AppException(ErrorCodes.VALIDATION_ERROR, "Validation failed.", 400);
        }

        var result = await _authService.LoginWithGoogleAsync(request.IdToken);
        SetTokenCookies(result.AccessToken, result.RefreshToken, result.ExpiresIn);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    // == POST /api/v1/auth/github ===
    [HttpPost("github")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> LoginWithGitHub([FromBody] GitHubLoginRequest request)
    {
        var validation = await _githubLoginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            throw new AppException(ErrorCodes.VALIDATION_ERROR, "Validation failed.", 400);
        }

        var result = await _authService.LoginWithGitHubAsync(request.Code);
        SetTokenCookies(result.AccessToken, result.RefreshToken, result.ExpiresIn);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    // == POST /api/v1/auth/apple ===
    [HttpPost("apple")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> LoginWithApple([FromBody] AppleLoginRequest request)
    {
        var validation = await _appleLoginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            throw new AppException(ErrorCodes.VALIDATION_ERROR, "Validation failed.", 400);
        }

        var result = await _authService.LoginWithAppleAsync(request.IdentityToken, request.FirstName, request.LastName);
        SetTokenCookies(result.AccessToken, result.RefreshToken, result.ExpiresIn);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    private void SetTokenCookies(string accessToken, string refreshToken, int accessExpiresInSeconds)
    {
        var isDevelopment = _env.IsDevelopment();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment, // Secure cookies only in non-development (requires HTTPS)
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/",
            MaxAge = TimeSpan.FromSeconds(accessExpiresInSeconds)
        };

        if (!isDevelopment)
        {
            cookieOptions.Extensions.Add("Partitioned");
        }

        Response.Cookies.Append("access_token", accessToken, cookieOptions);

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/",
            MaxAge = TimeSpan.FromDays(7) // Explicitly set MaxAge for refresh token
        };

        if (!isDevelopment)
        {
            refreshCookieOptions.Extensions.Add("Partitioned");
        }

        Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
    }

    private void ClearTokenCookies()
    {
        var isDevelopment = _env.IsDevelopment();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1) // Deleting cookie still uses Expires in the past
        };

        if (!isDevelopment)
        {
            cookieOptions.Extensions.Add("Partitioned");
        }

        Response.Cookies.Delete("access_token", cookieOptions);
        Response.Cookies.Delete("refresh_token", cookieOptions);
    }
}