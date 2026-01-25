using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StoryJudge.Core.DTOs;
using StoryJudge.Core.Enums;
using StoryJudge.Core.Interfaces;
using StoryJudge.Core.Models;

namespace StoryJudge.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<(User User, string Token)> RegisterAsync(string email, string password, string displayName)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Validate password strength
        if (password.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters");
        }

        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            OAuthProvider = OAuthProvider.Email,
            OAuthId = null,
            RoleLevel = RoleLevel.User,
            EmailVerified = false
        };

        user = await _userRepository.CreateAsync(user);
        var token = GenerateJwtToken(user);
        return (user, token);
    }

    public async Task<(User User, string Token)> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (user.OAuthProvider != OAuthProvider.Email || string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Please use OAuth login for this account");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (user.IsBanned)
        {
            throw new UnauthorizedAccessException("User is banned");
        }

        var token = GenerateJwtToken(user);
        return (user, token);
    }

    public async Task<(User User, string Token)> AuthenticateOAuthAsync(
        OAuthProvider provider,
        string oauthId,
        string email,
        string displayName,
        string? avatarUrl)
    {
        var user = await _userRepository.GetByOAuthAsync(provider, oauthId);

        if (user == null)
        {
            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already registered with a different provider");
            }

            user = new User
            {
                Email = email,
                DisplayName = displayName,
                AvatarUrl = avatarUrl,
                OAuthProvider = provider,
                OAuthId = oauthId,
                RoleLevel = RoleLevel.User,
                EmailVerified = true // OAuth emails are pre-verified
            };
            user = await _userRepository.CreateAsync(user);
        }
        else
        {
            // Update user info if changed
            var updated = false;
            if (user.DisplayName != displayName)
            {
                user.DisplayName = displayName;
                updated = true;
            }
            if (user.AvatarUrl != avatarUrl)
            {
                user.AvatarUrl = avatarUrl;
                updated = true;
            }
            if (updated)
            {
                user = await _userRepository.UpdateAsync(user);
            }
        }

        if (user.IsBanned)
        {
            throw new UnauthorizedAccessException("User is banned");
        }

        var token = GenerateJwtToken(user);
        return (user, token);
    }

    public string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "StoryJudge";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "StoryJudge";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.RoleLevel.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Username,
            user.AvatarUrl,
            user.Bio,
            user.RoleLevel.ToString(),
            user.ReputationScore,
            user.CreatedAt
        );
    }
}
