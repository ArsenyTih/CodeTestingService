using CodeTestingService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeTestingService.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext dbContext)
    {
        if (!await dbContext.Repositories.AnyAsync())
        {
            var repositories = new[]
            {
                new Repository { StudentId = 101, LabNumber = 1, RepoUrl = "https://gitlab.example/student101/lab1" },
                new Repository { StudentId = 101, LabNumber = 2, RepoUrl = "https://gitlab.example/student101/lab2" },
                new Repository { StudentId = 102, LabNumber = 1, RepoUrl = "https://gitlab.example/student102/lab1" },
                new Repository { StudentId = 103, LabNumber = 1, RepoUrl = "https://gitlab.example/student103/lab1" }
            };

            await dbContext.Repositories.AddRangeAsync(repositories);
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.CheckResults.AnyAsync())
        {
            var repositoriesByUrl = await dbContext.Repositories.ToDictionaryAsync(repository => repository.RepoUrl);
            var results = new List<CheckResult>();

            if (repositoriesByUrl.TryGetValue("https://gitlab.example/student101/lab1", out var student101Lab1))
            {
                results.Add(new CheckResult
                {
                    RepositoryId = student101Lab1.Id,
                    Status = "Passed",
                    Result = "Все тесты успешно пройдены.",
                    CheckedAt = DateTime.SpecifyKind(new DateTime(2026, 3, 20, 10, 0, 0), DateTimeKind.Utc)
                });
                results.Add(new CheckResult
                {
                    RepositoryId = student101Lab1.Id,
                    Status = "Passed",
                    Result = "Повторная проверка подтверждает успешный результат.",
                    CheckedAt = DateTime.SpecifyKind(new DateTime(2026, 3, 23, 9, 30, 0), DateTimeKind.Utc)
                });
            }

            if (repositoriesByUrl.TryGetValue("https://gitlab.example/student102/lab1", out var student102Lab1))
            {
                results.Add(new CheckResult
                {
                    RepositoryId = student102Lab1.Id,
                    Status = "Failed",
                    Result = "Не пройдены тесты на граничных значениях.",
                    CheckedAt = DateTime.SpecifyKind(new DateTime(2026, 3, 24, 11, 45, 0), DateTimeKind.Utc)
                });
            }

            if (repositoriesByUrl.TryGetValue("https://gitlab.example/student103/lab1", out var student103Lab1))
            {
                results.Add(new CheckResult
                {
                    RepositoryId = student103Lab1.Id,
                    Status = "Pending",
                    Result = "Проверка поставлена в очередь.",
                    CheckedAt = DateTime.SpecifyKind(new DateTime(2026, 3, 25, 8, 15, 0), DateTimeKind.Utc)
                });
            }

            if (results.Count > 0)
            {
                await dbContext.CheckResults.AddRangeAsync(results);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
