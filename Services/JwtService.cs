using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TicketManagementApi.Models;

namespace TicketManagementApi.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly List<UserCredential> _users;
    private readonly ConcurrentDictionary<string, RefreshTokenInfo> _refreshTokens = new();

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _users = _configuration.GetSection("Users").Get<List<UserCredential>>() ?? new List<UserCredential>();
    }
    // This method generates a JWT token for a given username and role. It reads the secret key, issuer, audience, and expiry settings from the configuration.
    public string GenerateToken(string username, string role)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] 
            ?? throw new InvalidOperationException("JWT Secret Key is not configured");
        var issuer = _configuration["JwtSettings:Issuer"];
        var audience = _configuration["JwtSettings:Audience"];
        var expiryMinutes = GetAccessTokenExpiryMinutes();

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        // The claims include the username, role, subject, JWT ID, and issued-at time. 
        // These claims will be included in the payload of the generated JWT token.
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    // This method validates the provided username and password against the in-memory list of users. If a match is found, it returns the corresponding UserCredential object; otherwise, it returns null.
    public UserCredential? ValidateUser(string username, string password)
    {
        return _users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
            u.Password == password);
    }

    public int GetAccessTokenExpiryMinutes()
    {
        return int.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60");
    }

    public int GetRefreshTokenExpiryMinutes()
    {
        return int.Parse(_configuration["JwtSettings:RefreshTokenExpiryInMinutes"] ?? "1440");
    }

    public (string Token, DateTime ExpiresAt) GenerateRefreshToken(string username, string role)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(tokenBytes);
        var expiresAt = DateTime.UtcNow.AddMinutes(GetRefreshTokenExpiryMinutes());

        var info = new RefreshTokenInfo(username, role, expiresAt);
        _refreshTokens[refreshToken] = info;

        return (refreshToken, expiresAt);
    }

    public UserCredential? ValidateRefreshToken(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var info))
        {
            return null;
        }

        if (info.ExpiresAt <= DateTime.UtcNow)
        {
            _refreshTokens.TryRemove(refreshToken, out _);
            return null;
        }

        return new UserCredential
        {
            Username = info.Username,
            Role = info.Role
        };
    }

    public void RevokeRefreshToken(string refreshToken)
    {
        _refreshTokens.TryRemove(refreshToken, out _);
    }

    private sealed record RefreshTokenInfo(string Username, string Role, DateTime ExpiresAt);
}
