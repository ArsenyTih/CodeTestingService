using System.Diagnostics;
using CodeTestingService.Models;
using CodeTestingService.Models.ViewModels;
using CodeTestingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeTestingService.Controllers;

public class HomeController : Controller
{
    private readonly DemoJwtUserService _demoJwtUserService;
    private readonly AccessService _accessService;
    private readonly RepositoryService _repositoryService;
    private readonly ResultsService _resultsService;

    public HomeController(
        DemoJwtUserService demoJwtUserService,
        AccessService accessService,
        RepositoryService repositoryService,
        ResultsService resultsService)
    {
        _demoJwtUserService = demoJwtUserService;
        _accessService = accessService;
        _repositoryService = repositoryService;
        _resultsService = resultsService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Student(int labNumber = 1)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, "Demo token is required.");
        }

        if (!string.Equals(currentUser.Role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var normalizedLabNumber = NormalizeLabNumber(labNumber);
        var repository = await _repositoryService.GetStudentRepositoryAsync(currentUser.UserId, normalizedLabNumber);
        var latestResult = await _resultsService.GetLatestStudentResultAsync(currentUser.UserId, normalizedLabNumber);

        var model = new StudentPageViewModel
        {
            CurrentUserName = currentUser.Name,
            StudentId = currentUser.UserId,
            LabNumber = normalizedLabNumber,
            Token = currentUser.Token,
            RepoUrl = repository?.RepoUrl,
            Status = latestResult?.Status,
            Result = latestResult?.Result,
            CheckedAt = latestResult?.CheckedAt
        };

        return View(model);
    }

    public async Task<IActionResult> Teacher(int labNumber = 1)
    {
        return await BuildStaffPageAsync("Teacher", "Teacher", labNumber);
    }

    public async Task<IActionResult> Assistant(int labNumber = 1)
    {
        return await BuildStaffPageAsync("Assistant", "Assistant", labNumber);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<IActionResult> BuildStaffPageAsync(string expectedRole, string viewName, int labNumber)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, "Demo token is required.");
        }

        if (!string.Equals(currentUser.Role, expectedRole, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var normalizedLabNumber = NormalizeLabNumber(labNumber);
        var accessibleStudentIds = await _accessService.GetAccessibleStudentIdsAsync(currentUser);
        var repositories = await _repositoryService.GetStudentsRepositoriesAsync(accessibleStudentIds, normalizedLabNumber);
        var results = await _resultsService.GetStudentsLatestResultsAsync(accessibleStudentIds, normalizedLabNumber);
        var testRepository = await _repositoryService.GetTestRepositoryAsync(normalizedLabNumber);

        var repositoryByStudentId = repositories.ToDictionary(item => item.StudentId);
        var resultByStudentId = results.ToDictionary(item => item.StudentId);
        var rows = accessibleStudentIds
            .OrderBy(studentId => studentId)
            .Select(studentId =>
            {
                repositoryByStudentId.TryGetValue(studentId, out var repository);
                resultByStudentId.TryGetValue(studentId, out var result);

                return new TeacherStudentItemViewModel
                {
                    StudentId = studentId,
                    StudentName = repository?.StudentName ?? result?.StudentName ?? $"Student {studentId}",
                    RepoUrl = repository?.RepoUrl,
                    Status = result?.Status,
                    Result = result?.Result,
                    CheckedAt = result?.CheckedAt
                };
            })
            .ToList();

        var model = new TeacherPageViewModel
        {
            CurrentUserName = currentUser.Name,
            CurrentUserRole = currentUser.Role,
            LabNumber = normalizedLabNumber,
            Token = currentUser.Token,
            TestRepositoryUrl = testRepository?.RepoUrl,
            Students = rows
        };

        return View(viewName, model);
    }

    private static int NormalizeLabNumber(int labNumber)
    {
        return labNumber > 0 ? labNumber : 1;
    }
}
