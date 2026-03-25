using System.Text.Json;
using CodeTestingService.Models.Demo;

namespace CodeTestingService.Services;

public class DemoCoreDataService
{
    private readonly IReadOnlyList<DemoUser> _users;
    private readonly IReadOnlyList<DemoStudentGroup> _teacherGroups;
    private readonly IReadOnlyList<DemoStudentGroup> _assistantGroups;
    private readonly IReadOnlyList<DemoTestRepository> _testRepositories;

    public DemoCoreDataService(IHostEnvironment hostEnvironment)
    {
        var dataDirectory = Path.Combine(hostEnvironment.ContentRootPath, "DemoData");
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _users = ReadJson<List<DemoUser>>(Path.Combine(dataDirectory, "demo-users.json"), serializerOptions);
        _teacherGroups = ReadJson<List<DemoStudentGroup>>(Path.Combine(dataDirectory, "teacher-students.json"), serializerOptions);
        _assistantGroups = ReadJson<List<DemoStudentGroup>>(Path.Combine(dataDirectory, "assistant-students.json"), serializerOptions);
        _testRepositories = ReadJson<List<DemoTestRepository>>(Path.Combine(dataDirectory, "test-repositories.json"), serializerOptions);
    }

    public Task<DemoUser?> GetUserByTokenAsync(string token)
    {
        var user = _users.FirstOrDefault(item => string.Equals(item.Token, token, StringComparison.Ordinal));
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<int>> GetTeacherStudentIdsAsync(int teacherId)
    {
        var studentIds = _teacherGroups
            .Where(group => group.TeacherId == teacherId)
            .SelectMany(group => group.StudentIds)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        return Task.FromResult<IReadOnlyList<int>>(studentIds);
    }

    public Task<IReadOnlyList<int>> GetAssistantStudentIdsAsync(int assistantId)
    {
        var studentIds = _assistantGroups
            .Where(group => group.AssistantId == assistantId)
            .SelectMany(group => group.StudentIds)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        return Task.FromResult<IReadOnlyList<int>>(studentIds);
    }

    public Task<string> GetStudentNameAsync(int studentId)
    {
        var name = _users
            .FirstOrDefault(user =>
                string.Equals(user.Role, "Student", StringComparison.OrdinalIgnoreCase) &&
                user.UserId == studentId)?.Name
            ?? $"Student {studentId}";

        return Task.FromResult(name);
    }

    public Task<DemoTestRepository?> GetTestRepositoryAsync(int labNumber)
    {
        var repository = _testRepositories.FirstOrDefault(item => item.LabNumber == labNumber);
        return Task.FromResult(repository);
    }

    private static T ReadJson<T>(string path, JsonSerializerOptions options)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Demo data file '{path}' was not found.");
        }

        var content = File.ReadAllText(path);
        var model = JsonSerializer.Deserialize<T>(content, options);
        if (model is null)
        {
            throw new InvalidOperationException($"Demo data file '{path}' has invalid JSON content.");
        }

        return model;
    }
}
