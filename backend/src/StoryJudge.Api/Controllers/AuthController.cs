using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryJudge.Core.DTOs;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Interfaces;

namespace StoryJudge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var (user, token) = await _authService.RegisterAsync(request.Email, request.Password, request.DisplayName);
            return Ok(new LoginResponse(token, new UserDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.Username,
                user.AvatarUrl,
                user.Bio,
                user.RoleLevel.ToString(),
                user.ReputationScore,
                user.CreatedAt
            )));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var (user, token) = await _authService.LoginAsync(request.Email, request.Password);
            return Ok(new LoginResponse(token, new UserDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.Username,
                user.AvatarUrl,
                user.Bio,
                user.RoleLevel.ToString(),
                user.ReputationScore,
                user.CreatedAt
            )));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("login/google")]
    public IActionResult LoginGoogle([FromQuery] string? returnUrl = null)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("callback/google")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            return Unauthorized("Google authentication failed");
        }

        var claims = result.Principal?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var oauthId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var picture = claims?.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(oauthId))
        {
            return BadRequest("Could not retrieve user information from Google");
        }

        var (user, token) = await _authService.AuthenticateOAuthAsync(
            OAuthProvider.Google,
            oauthId,
            email,
            name ?? email,
            picture
        );

        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
        return Redirect($"{frontendUrl}/auth/callback?token={token}");
    }

    [HttpGet("login/linkedin")]
    public IActionResult LoginLinkedIn([FromQuery] string? returnUrl = null)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(LinkedInCallback), new { returnUrl })
        };
        return Challenge(properties, "LinkedIn");
    }

    [HttpGet("callback/linkedin")]
    public async Task<IActionResult> LinkedInCallback([FromQuery] string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync("LinkedIn");
        if (!result.Succeeded)
        {
            return Unauthorized("LinkedIn authentication failed");
        }

        var claims = result.Principal?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var oauthId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var picture = claims?.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(oauthId))
        {
            return BadRequest("Could not retrieve user information from LinkedIn");
        }

        var (user, token) = await _authService.AuthenticateOAuthAsync(
            OAuthProvider.LinkedIn,
            oauthId,
            email,
            name ?? email,
            picture
        );

        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
        return Redirect($"{frontendUrl}/auth/callback?token={token}");
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(user);
    }
}
