using OpsFlow.Domain.Entities;

namespace OpsFlow.Application.Interfaces;

public interface ITokenService
{
  string CreateToken(User user);
}