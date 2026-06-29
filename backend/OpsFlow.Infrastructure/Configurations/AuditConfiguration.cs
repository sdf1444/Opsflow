using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
  public void Configure(EntityTypeBuilder<AuditLog> builder)
  {
    builder.ToTable("AuditLogs");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Action)
      .HasMaxLength(100)
      .IsRequired();

    builder.Property(x => x.Details)
      .HasMaxLength(4000);
  }
}