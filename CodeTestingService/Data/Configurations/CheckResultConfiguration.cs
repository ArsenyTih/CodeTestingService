using CodeTestingService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeTestingService.Data.Configurations;

public class CheckResultConfiguration : IEntityTypeConfiguration<CheckResult>
{
    public void Configure(EntityTypeBuilder<CheckResult> builder)
    {
        builder.ToTable("check_result");

        builder.HasKey(checkResult => checkResult.Id);

        builder.Property(checkResult => checkResult.Id)
            .HasColumnName("id");

        builder.Property(checkResult => checkResult.RepositoryId)
            .HasColumnName("repository_id")
            .IsRequired();

        builder.Property(checkResult => checkResult.Status)
            .HasColumnName("status")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(checkResult => checkResult.Result)
            .HasColumnName("result")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(checkResult => checkResult.CheckedAt)
            .HasColumnName("checked_at")
            .IsRequired();
    }
}
