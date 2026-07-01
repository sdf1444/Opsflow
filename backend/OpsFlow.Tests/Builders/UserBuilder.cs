using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;

namespace OpsFlow.Tests.Builders;

public class UserBuilder
{
    private readonly User _user;

    public UserBuilder()
    {
        _user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = $"user.{Guid.NewGuid():N}@test.local",
            PasswordHash = "hash",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public UserBuilder AsEmployee()
    {
        _user.Role = UserRole.Employee;
        return this;
    }

    public UserBuilder AsManager()
    {
        _user.Role = UserRole.Manager;
        return this;
    }

    public UserBuilder AsAdmin()
    {
        _user.Role = UserRole.Admin;
        return this;
    }

    public UserBuilder WithName(string name)
    {
        _user.Name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _user.PasswordHash = passwordHash;
        return this;
    }

    public User Build() => _user;
}
