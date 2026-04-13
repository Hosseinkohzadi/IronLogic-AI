using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Hangfire;

using IronLogic.Application.DTOs.Auth;
using IronLogic.Application.Interfaces;
using IronLogic.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace IronLogic.Api.Controllers;

/// <summary>
///     Controller for handling user authentication operations including registration, login, and logout
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<IdentityRole> roleManager,
    IBackgroundJobClient backgroundJobClient,
    IOtpService otpService,
    IConfiguration configuration,
    ILogger<AuthController> logger)
    : ControllerBase
{
    /// <summary>
    ///     Registers a new user and dispatches a six-digit verification code to their email address.
    ///     The JWT token is not issued at this stage; call <c>verify-email</c> after confirming the code.
    /// </summary>
    /// <param name="registerDto">Registration data containing email, password, and optional full name</param>
    /// <returns>User ID and a confirmation message indicating the verification code was sent</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        await EnsureRolesExistAsync();

        var user = new User
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
            EmailConfirmed = false
        };

        IdentityResult result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        IdentityResult roleResult = await userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded) logger.LogWarning("Failed to assign User role to {Email}", registerDto.Email);

        var (code, _) = await otpService.GenerateAsync(user.Id);

        backgroundJobClient.Enqueue<IEmailAutomationService>(service =>
            service.SendConfirmationCodeEmailAsync(user.Id, code, CancellationToken.None));

        return Ok(new
        {
            UserId = user.Id,
            Message = "Registration successful. A 6-digit verification code has been sent to your email."
        });
    }

    /// <summary>
    ///     Verifies the six-digit OTP code sent to the user's email address.
    ///     On success, confirms the email, issues a JWT token, and dispatches a welcome email.
    /// </summary>
    /// <param name="verifyEmailDto">Payload containing the user ID and the six-digit code</param>
    /// <returns>JWT token and user details on success; 400 on invalid or expired code</returns>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        User? user = await userManager.FindByIdAsync(verifyEmailDto.UserId);
        if (user is null)
            return NotFound(new { Message = "User not found." });

        if (user.EmailConfirmed)
            return BadRequest(new { Message = "Email address is already verified." });

        var token = await otpService.ValidateAndConsumeAsync(verifyEmailDto.UserId, verifyEmailDto.Code);
        if (token is null)
            return BadRequest(new { Message = "Invalid or expired verification code." });

        IdentityResult confirmResult = await userManager.ConfirmEmailAsync(user, token);
        if (!confirmResult.Succeeded)
        {
            logger.LogWarning("ConfirmEmailAsync failed for user {UserId}", user.Id);
            return BadRequest(new { Message = "Email confirmation failed. Please request a new code." });
        }

        backgroundJobClient.Enqueue<IEmailAutomationService>(service =>
            service.SendWelcomeEmailAsync(user.Id, CancellationToken.None));

        var jwtToken = await GenerateJwtTokenAsync(user);
        IList<string> roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var response = new AuthResponseDto(
            jwtToken,
            Guid.Parse(user.Id),
            user.Email ?? string.Empty,
            user.UserName,
            role
        );

        logger.LogInformation("Email verified successfully for user {UserId}", user.Id);
        return Ok(response);
    }

    /// <summary>
    ///     Authenticates a user with email and password and returns a JWT token
    /// </summary>
    /// <param name="loginDto">Login credentials containing email and password</param>
    /// <returns>JWT token and user details if authentication is successful</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        User? user = await userManager.FindByEmailAsync(loginDto.Email);

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

        SignInResult result = await signInManager.PasswordSignInAsync(
            user.UserName ?? loginDto.Email,
            loginDto.Password,
            false,
            false);

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed: SignInManager returned unsuccessful result for {Email}", loginDto.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        var token = await GenerateJwtTokenAsync(user);
        IList<string> roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var response = new AuthResponseDto(
            token,
            Guid.Parse(user.Id),
            user.Email ?? string.Empty,
            user.UserName,
            role
        );

        return Ok(response);
    }

    /// <summary>
    ///     Signs out the current user
    /// </summary>
    /// <returns>Success message confirming logout</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok(new { Message = "Logout successful" });
    }

    /// <summary>
    ///     Generates a JWT token for the authenticated user with role claims
    /// </summary>
    /// <param name="user">The user entity to generate token for</param>
    /// <returns>JWT token as a string</returns>
    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        IList<string> roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role claims
        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ??
                                                                  throw new InvalidOperationException(
                                                                      "JWT Key is missing")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        DateTime expires = DateTime.UtcNow.AddDays(Convert.ToDouble(configuration["Jwt:ExpireDays"] ?? "1"));

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    ///     Ensures that Admin and User roles exist in the database
    /// </summary>
    private async Task EnsureRolesExistAsync()
    {
        string[] roles = ["Admin", "User"];

        foreach (var roleName in roles)
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Created role: {RoleName}", roleName);
            }
    }
}
