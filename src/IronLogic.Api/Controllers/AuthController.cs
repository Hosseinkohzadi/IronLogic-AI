using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IronLogic.Application.DTOs.Auth;
using IronLogic.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace IronLogic.Api.Controllers;

/// <summary>
/// Controller for handling user authentication operations including registration, login, and logout
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IConfiguration configuration,
    ILogger<AuthController> logger)
    : ControllerBase
{
    /// <summary>
    /// Registers a new user with email and password
    /// </summary>
    /// <param name="registerDto">Registration data containing email and password</param>
    /// <returns>Success message with the newly created user ID</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var user = new User
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Message = "Registration successful", UserId = user.Id });
    }

    /// <summary>
    /// Authenticates a user with email and password and returns a JWT token
    /// </summary>
    /// <param name="loginDto">Login credentials containing email and password</param>
    /// <returns>JWT token and user details if authentication is successful</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        
        if (user == null)
        {
            logger.LogWarning("Login failed: User {Email} not found", loginDto.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        var passwordCheck = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordCheck)
        {
            logger.LogWarning("Login failed: Wrong password for user {Email}", loginDto.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName ?? loginDto.Email,
            loginDto.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed: SignInManager returned unsuccessful result for {Email}", loginDto.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        var token = GenerateJwtToken(user);

        var response = new AuthResponseDto(token, Guid.Parse(user.Id), user.Email ?? string.Empty, user.UserName);
        return Ok(response);
    }

    /// <summary>
    /// Signs out the current user
    /// </summary>
    /// <returns>Success message confirming logout</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok(new { Message = "Logout successful" });
    }

    /// <summary>
    /// Generates a JWT token for the authenticated user
    /// </summary>
    /// <param name="user">The user entity to generate token for</param>
    /// <returns>JWT token as a string</returns>
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(configuration["Jwt:ExpireDays"] ?? "1"));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}