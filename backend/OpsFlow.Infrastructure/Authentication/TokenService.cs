using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Infrastructure.Authentication;

public class TokenService : ITokenService
{
  private readonly JwtSettings _jwtSettings;

  public TokenService(IOptions<JwtSettings> jwtSettings)
  {
    _jwtSettings = jwtSettings.Value;
  }

  public string CreateToken(User user)
  {
    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new(ClaimTypes.Email, user.Email),
      new(ClaimTypes.Name, user.Name),
      new(ClaimTypes.Role, user.Role.ToString())
    };

    var key = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(_jwtSettings.Key));

    var credentials = new SigningCredentials(
      key,
      SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: _jwtSettings.Issuer,
      audience: _jwtSettings.Audience,
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
      signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}