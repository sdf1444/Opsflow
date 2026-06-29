using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Name)
      .HasMaxLength(200)
      .IsRequired();

    builder.Property(x => x.Email)
      .HasMaxLength(255)
      .IsRequired();

    builder.HasIndex(x => x.Email)
      .IsUnique();

    builder.Property(x => x.PasswordHash)
      .IsRequired();

    builder.Property(x => x.Role)
      .IsRequired();
  }
}