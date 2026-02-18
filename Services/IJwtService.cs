using TicketManagementApi.Models;

namespace TicketManagementApi.Services;

public interface IJwtService
{
    string GenerateToken(string username, string role);
    UserCredential? ValidateUser(string username, string password);
    int GetAccessTokenExpiryMinutes();
    int GetRefreshTokenExpiryMinutes();
    (string Token, DateTime ExpiresAt) GenerateRefreshToken(string username, string role);
    UserCredential? ValidateRefreshToken(string refreshToken);
    void RevokeRefreshToken(string refreshToken);
}
