using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.DTOs.Auth;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public AuthController(
        AppDbContext dbContext,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _dbContext.Users
            .AnyAsync(x => x.Email == email, cancellationToken);

        if (emailExists)
        {
            return BadRequest("A user with this email already exists.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            return BadRequest("Invalid user role.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = _tokenService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString()
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = _tokenService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString()
        });
    }

    [HttpGet("me")]
    [Authorize(Policy = "AuthenticatedUser")]    
    public async Task<ActionResult<CurrentUserResponse>> Me(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString()
        });
    }
}