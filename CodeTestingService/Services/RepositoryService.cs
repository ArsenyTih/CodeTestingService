using CodeTestingService.Data;
using CodeTestingService.Data.Entities;
using CodeTestingService.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace CodeTestingService.Services;

public class RepositoryService
{
    private readonly AppDbContext _dbContext;
    private readonly DemoCoreDataService _demoCoreDataService;

    public RepositoryService(AppDbContext dbContext, DemoCoreDataService demoCoreDataService)
    {
        _dbContext = dbContext;
        _demoCoreDataService = demoCoreDataService;
    }

    public async Task<RepositoryResponse> UpsertRepositoryAsync(int studentId, int labNumber, string repoUrl)
    {
        var normalizedRepoUrl = repoUrl.Trim();
        var repository = await _dbContext.Repositories
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.LabNumber == labNumber);

        if (repository is null)
        {
            repository = new Repository
            {
                StudentId = studentId,
                LabNumber = labNumber,
                RepoUrl = normalizedRepoUrl
            };

            _dbContext.Repositories.Add(repository);
        }
        else
        {
            repository.RepoUrl = normalizedRepoUrl;
        }

        await _dbContext.SaveChangesAsync();

        return new RepositoryResponse
        {
            StudentId = repository.StudentId,
            LabNumber = repository.LabNumber,
            RepoUrl = repository.RepoUrl
        };
    }

    public async Task<RepositoryResponse?> GetStudentRepositoryAsync(int studentId, int labNumber)
    {
        return await _dbContext.Repositories
            .AsNoTracking()
            .Where(item => item.StudentId == studentId && item.LabNumber == labNumber)
            .Select(item => new RepositoryResponse
            {
                StudentId = item.StudentId,
                LabNumber = item.LabNumber,
                RepoUrl = item.RepoUrl
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<StudentRepositoryResponse>> GetStudentsRepositoriesAsync(IEnumerable<int> studentIds, int labNumber)
    {
        var distinctStudentIds = studentIds
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var repositoryByStudentId = await _dbContext.Repositories
            .AsNoTracking()
            .Where(item => item.LabNumber == labNumber && distinctStudentIds.Contains(item.StudentId))
            .ToDictionaryAsync(item => item.StudentId, item => item.RepoUrl);

        var result = new List<StudentRepositoryResponse>(distinctStudentIds.Count);
        foreach (var studentId in distinctStudentIds)
        {
            result.Add(new StudentRepositoryResponse
            {
                StudentId = studentId,
                StudentName = await _demoCoreDataService.GetStudentNameAsync(studentId),
                LabNumber = labNumber,
                RepoUrl = repositoryByStudentId.TryGetValue(studentId, out var repoUrl) ? repoUrl : null
            });
        }

        return result;
    }

    public async Task<RepositoryResponse?> GetTestRepositoryAsync(int labNumber)
    {
        var testRepository = await _demoCoreDataService.GetTestRepositoryAsync(labNumber);
        if (testRepository is null)
        {
            return null;
        }

        return new RepositoryResponse
        {
            StudentId = 0,
            LabNumber = testRepository.LabNumber,
            RepoUrl = testRepository.RepoUrl
        };
    }
}
