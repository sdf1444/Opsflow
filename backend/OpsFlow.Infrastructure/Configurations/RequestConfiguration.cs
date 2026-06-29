using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Infrastructure.Persistence.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
  public void Configure(EntityTypeBuilder<Request> builder)
  {
    builder.ToTable("Requests");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Title)
      .HasMaxLength(200)
      .IsRequired();

    builder.Property(x => x.Description)
      .HasMaxLength(5000)
      .IsRequired();

    builder.Property(x => x.Status)
      .IsRequired();

    builder.Property(x => x.Category)
      .IsRequired();

    builder.HasOne(x => x.CreatedByUser)
      .WithMany(x => x.RequestsCreated)
      .HasForeignKey(x => x.CreatedByUserId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(x => x.AssignedReviewer)
      .WithMany(x => x.RequestsAssigned)
      .HasForeignKey(x => x.AssignedReviewerId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}