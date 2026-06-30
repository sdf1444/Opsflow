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

    builder.HasOne(x => x.Request)
      .WithMany(x => x.Comments)
      .HasForeignKey(x => x.RequestId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(x => x.User)
      .WithMany(x => x.Comments)
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}