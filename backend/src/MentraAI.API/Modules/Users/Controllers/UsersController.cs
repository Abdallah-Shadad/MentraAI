using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Models;
using MentraAI.API.Modules.Users.DTOs.Requests;
using MentraAI.API.Modules.Users.DTOs.Responses;
using MentraAI.API.Modules.Users.Services;

namespace MentraAI.API.Modules.Users.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UpdateProfileRequest> _updateValidator;

    public UsersController(
        IUserService userService,
        IValidator<UpdateProfileRequest> updateValidator)
    {
        _userService = userService;
        _updateValidator = updateValidator;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // =====================================================================
    // GET /api/v1/users/me
    // =====================================================================
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetMe()
    {
        var result = await _userService.GetProfileAsync(GetUserId());
        return Ok(ApiResponse<UserProfileResponse>.Ok(result));
    }

    // =====================================================================
    // PUT /api/v1/users/me
    // =====================================================================
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new
            {
                success = false,
                error = new
                {
                    code = ErrorCodes.VALIDATION_ERROR,
                    message = "Validation failed.",
                    statusCode = 400,
                    errors
                }
            });
        }

        var result = await _userService.UpdateProfileAsync(GetUserId(), request);
        return Ok(ApiResponse<UserProfileResponse>.Ok(result));
    }
}