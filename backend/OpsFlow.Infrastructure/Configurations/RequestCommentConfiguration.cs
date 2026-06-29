using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

public class RequestCommentConfiguration : IEntityTypeConfiguration<RequestComment>
{
  public void Configure(EntityTypeBuilder<RequestComment> builder)
  {
    builder.ToTable("RequestComments");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Body)
      .HasMaxLength(2000)
      .IsRequired();
  }
}