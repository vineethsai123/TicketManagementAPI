using Microsoft.AspNetCore.Mvc;
using TicketManagementApi.Models;
using TicketManagementApi.Services;

namespace TicketManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    
    /// Login endpoint to authenticate user and generate JWT token
    /// <returns>JWT token if credentials are valid</returns>
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        var user = _jwtService.ValidateUser(request.Username, request.Password);

        if (user == null)
        {
            _logger.LogWarning("Invalid login attempt for user: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid username or password." });
        }
        // Generate a JWT token with the user's username and role as claims.
        var token = _jwtService.GenerateToken(user.Username, user.Role);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetAccessTokenExpiryMinutes());
        // Generate a long-lived refresh token used to get a new access token later.
        var refresh = _jwtService.GenerateRefreshToken(user.Username, user.Role);

        _logger.LogInformation("User {Username} logged in successfully", request.Username);
        // The response includes the generated JWT token, the username, role, and token expiration time. 
        // This information can be used by the client to manage authentication state and access protected resources.
        return Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            ExpiresAt = expiresAt,
            RefreshToken = refresh.Token,
            RefreshExpiresAt = refresh.ExpiresAt
        });
    }
    //function takes a refresh token from the client, validates it, 
    // and if valid, generates a new access token and a new refresh token.
    /// Exchange a refresh token for a new access token
    /// <returns>New JWT token if refresh token is valid</returns>
    [HttpPost("refresh")]
    public ActionResult<LoginResponse> Refresh([FromBody] RefreshRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required." });
        }

        // Validate the refresh token and map it back to a user.
        var user = _jwtService.ValidateRefreshToken(request.RefreshToken);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        } 

        // Rotate refresh tokens so the old token can no longer be used.
        _jwtService.RevokeRefreshToken(request.RefreshToken);

        var newAccessToken = _jwtService.GenerateToken(user.Username, user.Role);
        var accessExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetAccessTokenExpiryMinutes());
        var newRefresh = _jwtService.GenerateRefreshToken(user.Username, user.Role);

        return Ok(new LoginResponse
        {
            Token = newAccessToken,
            Username = user.Username,
            Role = user.Role,
            ExpiresAt = accessExpiresAt,
            RefreshToken = newRefresh.Token,
            RefreshExpiresAt = newRefresh.ExpiresAt
        });
    }
}
