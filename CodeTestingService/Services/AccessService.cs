using CodeTestingService.Models.Demo;

namespace CodeTestingService.Services;

public class AccessService
{
    private readonly DemoCoreDataService _demoCoreDataService;

    public AccessService(DemoCoreDataService demoCoreDataService)
    {
        _demoCoreDataService = demoCoreDataService;
    }

    public async Task<IReadOnlyList<int>> GetAccessibleStudentIdsAsync(CurrentUser currentUser)
    {
        if (string.Equals(currentUser.Role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { currentUser.UserId };
        }

        if (string.Equals(currentUser.Role, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            return await _demoCoreDataService.GetTeacherStudentIdsAsync(currentUser.UserId);
        }

        if (string.Equals(currentUser.Role, "Assistant", StringComparison.OrdinalIgnoreCase))
        {
            return await _demoCoreDataService.GetAssistantStudentIdsAsync(currentUser.UserId);
        }

        return Array.Empty<int>();
    }

    public async Task<bool> CanAccessStudentAsync(CurrentUser currentUser, int studentId)
    {
        var accessibleStudentIds = await GetAccessibleStudentIdsAsync(currentUser);
        return accessibleStudentIds.Contains(studentId);
    }

    public Task<string> GetStudentNameAsync(int studentId)
    {
        return _demoCoreDataService.GetStudentNameAsync(studentId);
    }

    public static bool IsStaffRole(string role)
    {
        return string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(role, "Assistant", StringComparison.OrdinalIgnoreCase);
    }
}
