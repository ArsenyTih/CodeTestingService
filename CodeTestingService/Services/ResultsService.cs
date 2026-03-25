using CodeTestingService.Data;
using CodeTestingService.Data.Entities;
using CodeTestingService.Infrastructure;
using CodeTestingService.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace CodeTestingService.Services;

public class ResultsService
{
    private readonly AppDbContext _dbContext;
    private readonly DemoCoreDataService _demoCoreDataService;

    public ResultsService(AppDbContext dbContext, DemoCoreDataService demoCoreDataService)
    {
        _dbContext = dbContext;
        _demoCoreDataService = demoCoreDataService;
    }

    public async Task<CheckResultResponse?> SaveGitLabResultAsync(string projectUrl, string report)
    {
        var normalizedRepoUrl = RepositoryUrlNormalizer.Normalize(projectUrl);
        if (string.IsNullOrWhiteSpace(normalizedRepoUrl) || string.IsNullOrWhiteSpace(report))
        {
            return null;
        }

        var repository = await _dbContext.Repositories
            .FirstOrDefaultAsync(item => item.RepoUrl == normalizedRepoUrl || item.RepoUrl == normalizedRepoUrl + "/");

        if (repository is null)
        {
            return null;
        }

        var checkResult = new CheckResult
        {
            RepositoryId = repository.Id,
            Status = "Warning",
            Result = report.Trim(),
            CheckedAt = DateTime.UtcNow
        };

        _dbContext.CheckResults.Add(checkResult);
        await _dbContext.SaveChangesAsync();

        return MapCheckResult(repository, checkResult);
    }

    public async Task<CheckResultResponse?> GetLatestStudentResultAsync(int studentId, int labNumber)
    {
        return await _dbContext.CheckResults
            .AsNoTracking()
            .Where(item => item.Repository.StudentId == studentId && item.Repository.LabNumber == labNumber)
            .OrderByDescending(item => item.CheckedAt)
            .ThenByDescending(item => item.Id)
            .Select(item => new CheckResultResponse
            {
                Id = item.Id,
                StudentId = item.Repository.StudentId,
                LabNumber = item.Repository.LabNumber,
                RepoUrl = item.Repository.RepoUrl,
                Status = item.Status,
                Result = item.Result,
                CheckedAt = item.CheckedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<CheckResultResponse>> GetStudentHistoryAsync(int studentId, int labNumber)
    {
        return await _dbContext.CheckResults
            .AsNoTracking()
            .Where(item => item.Repository.StudentId == studentId && item.Repository.LabNumber == labNumber)
            .OrderByDescending(item => item.CheckedAt)
            .ThenByDescending(item => item.Id)
            .Select(item => new CheckResultResponse
            {
                Id = item.Id,
                StudentId = item.Repository.StudentId,
                LabNumber = item.Repository.LabNumber,
                RepoUrl = item.Repository.RepoUrl,
                Status = item.Status,
                Result = item.Result,
                CheckedAt = item.CheckedAt
            })
            .ToListAsync();
    }

    public async Task<List<StudentResultResponse>> GetStudentsLatestResultsAsync(IEnumerable<int> studentIds, int labNumber)
    {
        var distinctStudentIds = studentIds
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var repositories = await _dbContext.Repositories
            .AsNoTracking()
            .Where(item => item.LabNumber == labNumber && distinctStudentIds.Contains(item.StudentId))
            .Select(item => new
            {
                item.Id,
                item.StudentId,
                item.RepoUrl
            })
            .ToListAsync();

        var latestResults = await _dbContext.CheckResults
            .AsNoTracking()
            .Where(item => item.Repository.LabNumber == labNumber && distinctStudentIds.Contains(item.Repository.StudentId))
            .GroupBy(item => item.RepositoryId)
            .Select(group => group
                .OrderByDescending(item => item.CheckedAt)
                .ThenByDescending(item => item.Id)
                .Select(item => new
                {
                    item.RepositoryId,
                    item.Status,
                    item.Result,
                    item.CheckedAt
                })
                .First())
            .ToListAsync();

        var repositoryByStudentId = repositories.ToDictionary(item => item.StudentId);
        var latestResultByRepositoryId = latestResults.ToDictionary(item => item.RepositoryId);
        var result = new List<StudentResultResponse>(distinctStudentIds.Count);

        foreach (var studentId in distinctStudentIds)
        {
            repositoryByStudentId.TryGetValue(studentId, out var repository);

            string? status = null;
            string? message = null;
            DateTime? checkedAt = null;
            if (repository is not null && latestResultByRepositoryId.TryGetValue(repository.Id, out var latestResult))
            {
                status = latestResult.Status;
                message = latestResult.Result;
                checkedAt = latestResult.CheckedAt;
            }

            result.Add(new StudentResultResponse
            {
                StudentId = studentId,
                StudentName = await _demoCoreDataService.GetStudentNameAsync(studentId),
                LabNumber = labNumber,
                RepoUrl = repository?.RepoUrl,
                Status = status,
                Result = message,
                CheckedAt = checkedAt
            });
        }

        return result;
    }

    private static CheckResultResponse MapCheckResult(Repository repository, CheckResult checkResult)
    {
        return new CheckResultResponse
        {
            Id = checkResult.Id,
            StudentId = repository.StudentId,
            LabNumber = repository.LabNumber,
            RepoUrl = repository.RepoUrl,
            Status = checkResult.Status,
            Result = checkResult.Result,
            CheckedAt = checkResult.CheckedAt
        };
    }
}
