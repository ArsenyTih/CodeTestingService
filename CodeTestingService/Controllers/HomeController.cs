using System.Diagnostics;
using CodeTestingService.Models;
using CodeTestingService.Models.Responses;
using CodeTestingService.Models.ViewModels;
using CodeTestingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeTestingService.Controllers;

public class HomeController : Controller
{
    private readonly DemoJwtUserService _demoJwtUserService;
    private readonly DemoCoreDataService _demoCoreDataService;
    private readonly AccessService _accessService;
    private readonly RepositoryService _repositoryService;
    private readonly ResultsService _resultsService;

    public HomeController(
        DemoJwtUserService demoJwtUserService,
        DemoCoreDataService demoCoreDataService,
        AccessService accessService,
        RepositoryService repositoryService,
        ResultsService resultsService)
    {
        _demoJwtUserService = demoJwtUserService;
        _demoCoreDataService = demoCoreDataService;
        _accessService = accessService;
        _repositoryService = repositoryService;
        _resultsService = resultsService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Student(int labNumber = 1, int? selectedResultId = null)
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

        var availableLabs = await GetAvailableLabsAsync();
        var normalizedLabNumber = NormalizeLabNumber(labNumber, availableLabs);
        var selectedRepository = await _repositoryService.GetStudentRepositoryAsync(currentUser.UserId, normalizedLabNumber);
        var selectedHistory = await _resultsService.GetStudentHistoryAsync(currentUser.UserId, normalizedLabNumber);
        var selectedResult = SelectResult(selectedHistory, selectedResultId);

        var labs = new List<LabSummaryItemViewModel>(availableLabs.Count);
        foreach (var availableLab in availableLabs)
        {
            var latestResult = await _resultsService.GetLatestStudentResultAsync(currentUser.UserId, availableLab);
            labs.Add(new LabSummaryItemViewModel
            {
                LabNumber = availableLab,
                IsActive = availableLab == normalizedLabNumber,
                Status = latestResult?.Status,
                CheckedAt = latestResult?.CheckedAt
            });
        }

        var model = new StudentPageViewModel
        {
            CurrentUserName = currentUser.Name,
            StudentId = currentUser.UserId,
            LabNumber = normalizedLabNumber,
            Token = currentUser.Token,
            RepoUrl = selectedRepository?.RepoUrl,
            Labs = labs,
            Results = selectedHistory
                .Select(item => new ResultSelectionItemViewModel
                {
                    Id = item.Id,
                    Status = item.Status,
                    CheckedAt = item.CheckedAt,
                    Result = item.Result
                })
                .ToList(),
            SelectedResultId = selectedResult?.Id,
            SelectedResultStatus = selectedResult?.Status,
            SelectedResultContent = selectedResult?.Result,
            SelectedResultCheckedAt = selectedResult?.CheckedAt
        };

        return View(model);
    }

    public async Task<IActionResult> Teacher(int labNumber = 1, int? studentId = null, int? selectedResultId = null)
    {
        return await BuildStaffPageAsync("Teacher", "Teacher", labNumber, studentId, selectedResultId);
    }

    public async Task<IActionResult> Assistant(int labNumber = 1, int? studentId = null, int? selectedResultId = null)
    {
        return await BuildStaffPageAsync("Assistant", "Assistant", labNumber, studentId, selectedResultId);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<IActionResult> BuildStaffPageAsync(
        string expectedRole,
        string viewName,
        int labNumber,
        int? studentId,
        int? selectedResultId)
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

        var availableLabs = await GetAvailableLabsAsync();
        var normalizedLabNumber = NormalizeLabNumber(labNumber, availableLabs);
        var accessibleStudentIds = await _accessService.GetAccessibleStudentIdsAsync(currentUser);
        var repositories = await _repositoryService.GetStudentsRepositoriesAsync(accessibleStudentIds, normalizedLabNumber);
        var results = await _resultsService.GetStudentsLatestResultsAsync(accessibleStudentIds, normalizedLabNumber);
        var testRepository = await _repositoryService.GetTestRepositoryAsync(normalizedLabNumber);

        var resolvedStudentId = studentId.HasValue && accessibleStudentIds.Contains(studentId.Value)
            ? studentId.Value
            : accessibleStudentIds.FirstOrDefault();

        var repositoryByStudentId = repositories.ToDictionary(item => item.StudentId);
        var resultByStudentId = results.ToDictionary(item => item.StudentId);
        var rows = accessibleStudentIds
            .OrderBy(id => id)
            .Select(id =>
            {
                repositoryByStudentId.TryGetValue(id, out var repository);
                resultByStudentId.TryGetValue(id, out var result);

                return new TeacherStudentItemViewModel
                {
                    StudentId = id,
                    StudentName = repository?.StudentName ?? result?.StudentName ?? $"Student {id}",
                    RepoUrl = repository?.RepoUrl,
                    Status = result?.Status,
                    CheckedAt = result?.CheckedAt,
                    IsActive = id == resolvedStudentId
                };
            })
            .ToList();

        List<CheckResultResponse> selectedHistory = new();
        if (resolvedStudentId != 0)
        {
            selectedHistory = await _resultsService.GetStudentHistoryAsync(resolvedStudentId, normalizedLabNumber);
        }

        var selectedResult = SelectResult(selectedHistory, selectedResultId);
        repositoryByStudentId.TryGetValue(resolvedStudentId, out var selectedRepository);

        var model = new TeacherPageViewModel
        {
            CurrentUserName = currentUser.Name,
            CurrentUserRole = currentUser.Role,
            ActionName = viewName,
            LabNumber = normalizedLabNumber,
            Token = currentUser.Token,
            TestRepositoryUrl = testRepository?.RepoUrl,
            Labs = availableLabs
                .Select(number => new LabOptionItemViewModel
                {
                    LabNumber = number,
                    IsActive = number == normalizedLabNumber
                })
                .ToList(),
            Students = rows,
            SelectedStudentId = resolvedStudentId == 0 ? null : resolvedStudentId,
            SelectedStudentName = rows.FirstOrDefault(item => item.StudentId == resolvedStudentId)?.StudentName,
            SelectedStudentRepoUrl = selectedRepository?.RepoUrl,
            Results = selectedHistory
                .Select(item => new ResultSelectionItemViewModel
                {
                    Id = item.Id,
                    Status = item.Status,
                    CheckedAt = item.CheckedAt,
                    Result = item.Result
                })
                .ToList(),
            SelectedResultId = selectedResult?.Id,
            SelectedResultStatus = selectedResult?.Status,
            SelectedResultContent = selectedResult?.Result,
            SelectedResultCheckedAt = selectedResult?.CheckedAt
        };

        return View(viewName, model);
    }

    private async Task<List<int>> GetAvailableLabsAsync()
    {
        var availableLabs = (await _demoCoreDataService.GetAvailableLabNumbersAsync()).ToList();
        if (availableLabs.Count == 0)
        {
            availableLabs.Add(1);
        }

        return availableLabs;
    }

    private static int NormalizeLabNumber(int labNumber, IReadOnlyList<int> availableLabs)
    {
        if (availableLabs.Contains(labNumber))
        {
            return labNumber;
        }

        return availableLabs[0];
    }

    private static CheckResultResponse? SelectResult(IEnumerable<CheckResultResponse> history, int? selectedResultId)
    {
        if (selectedResultId.HasValue)
        {
            var selected = history.FirstOrDefault(item => item.Id == selectedResultId.Value);
            if (selected is not null)
            {
                return selected;
            }
        }

        return history.FirstOrDefault();
    }
}
