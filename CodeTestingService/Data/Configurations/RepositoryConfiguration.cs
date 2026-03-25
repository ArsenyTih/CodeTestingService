using CodeTestingService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeTestingService.Data.Configurations;

public class RepositoryConfiguration : IEntityTypeConfiguration<Repository>
{
    public void Configure(EntityTypeBuilder<Repository> builder)
    {
        builder.ToTable("repository");

        builder.HasKey(repository => repository.Id);

        builder.Property(repository => repository.Id)
            .HasColumnName("id");

        builder.Property(repository => repository.StudentId)
            .HasColumnName("student_id")
            .IsRequired();

        builder.Property(repository => repository.LabNumber)
            .HasColumnName("lab_number")
            .IsRequired();

        builder.Property(repository => repository.RepoUrl)
            .HasColumnName("repo_url")
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(repository => new { repository.StudentId, repository.LabNumber })
            .IsUnique();

        builder.HasMany(repository => repository.CheckResults)
            .WithOne(checkResult => checkResult.Repository)
            .HasForeignKey(checkResult => checkResult.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
