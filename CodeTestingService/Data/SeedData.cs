using CodeTestingService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CodeTestingService.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext dbContext)
    {
        const string finalReportResourceName = "CodeTestingService.Resources.final_report.txt";

        if (!await dbContext.Repositories.AnyAsync())
        {
            var repositories = new[] // https://gitlab.com/hse-programming-2026/labs-students/lab-9/lab-9-v-10-iiivanov
            {
                new Repository { StudentId = 101, LabNumber = 9, RepoUrl = "https://gitlab.com/hse-programming-2026/labs-students/lab-9/lab-9-v-10-iiivanov" },
                new Repository { StudentId = 101, LabNumber = 10, RepoUrl = "https://gitlab.com/hse-programming-2026/labs-students/lab-10/lab-10-v-10-iiivanov" },
                new Repository { StudentId = 102, LabNumber = 9, RepoUrl = "https://gitlab.com/hse-programming-2026/labs-students/lab-9/lab-9-v-10-aapetrov" },
                new Repository { StudentId = 102, LabNumber = 10, RepoUrl = "https://gitlab.com/hse-programming-2026/labs-students/lab-10/lab-10-v-10-aapetrov" },
                new Repository { StudentId = 103, LabNumber = 9, RepoUrl = "https://gitlab.com/hse-programming-2026/labs-students/lab-9/lab-9-v-10-nnsidorov" },
                new Repository { StudentId = 103, LabNumber = 10, RepoUrl = "https://gitlab.com/hse-programming-2026/labs-students/lab-10/lab-10-v-10-nnsidorov" }
            };

            await dbContext.Repositories.AddRangeAsync(repositories);
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.CheckResults.AnyAsync())
        {
            var repositoriesByUrl = await dbContext.Repositories.ToDictionaryAsync(repository => repository.RepoUrl);
            var results = new List<CheckResult>();
            var finalReport = LoadEmbeddedTextResource(finalReportResourceName);

            // if (repositoriesByUrl.TryGetValue("https://gitlab.com/hse-programming-2026/labs-students/lab-9/lab-9-v-10-iiivanov", out var student101Lab9))
            // {
            //     results.Add(new CheckResult
            //     {
            //         RepositoryId = student101Lab9.Id,
            //         Status = "Failed",
            //         CheckedAt = DateTime.SpecifyKind(new DateTime(2026, 3, 24, 20, 0, 0), DateTimeKind.Utc)
            //     });
            // }

            if (repositoriesByUrl.TryGetValue("https://gitlab.com/hse-programming-2026/labs-students/lab-9/lab-9-v-10-aapetrov", out var student102Lab9))
            {
                results.Add(new CheckResult
                {
                    RepositoryId = student102Lab9.Id,
                    Status = "Warning",
                    Result = finalReport,
                    CheckedAt = DateTime.SpecifyKind(new DateTime(2026, 3, 24, 11, 45, 0), DateTimeKind.Utc)
                });
            }

            // if (repositoriesByUrl.TryGetValue("https://gitlab.com/hse-programming-2026/labs-students/lab-10/lab-10-v-10-nnsidorov", out var student103Lab10))
            // {
            //     results.Add(new CheckResult
            //     {
            //         RepositoryId = student103Lab10.Id,
            //         Status = "Failed",
            //         CheckedAt = DateTime.SpecifyKind(new DateTime(2026, 3, 24, 20, 0, 0), DateTimeKind.Utc)
            //     });
            // }

            // if (repositoriesByUrl.TryGetValue("https://gitlab.example/student103/lab1", out var student103Lab1))
            // {
            //     results.Add(new CheckResult
            //     {
            //         RepositoryId = student103Lab1.Id,
            //         Status = "Pending",
            //         Result = "Проверка поставлена в очередь.",
            //         CheckedAt = DateTime.SpecifyKind(new DateTime(2026, 3, 25, 8, 15, 0), DateTimeKind.Utc)
            //     });
            // }

            if (results.Count > 0)
            {
                await dbContext.CheckResults.AddRangeAsync(results);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private static string LoadEmbeddedTextResource(string resourceName)
    {
        var assembly = typeof(SeedData).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' was not found. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
