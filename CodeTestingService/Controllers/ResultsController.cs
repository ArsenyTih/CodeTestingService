using CodeTestingService.Models.ViewModels;
using CodeTestingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeTestingService.Controllers;

public class ResultsController : Controller
{
    private readonly DemoJwtUserService _demoJwtUserService;
    private readonly AccessService _accessService;
    private readonly ResultsService _resultsService;

    public ResultsController(
        DemoJwtUserService demoJwtUserService,
        AccessService accessService,
        ResultsService resultsService)
    {
        _demoJwtUserService = demoJwtUserService;
        _accessService = accessService;
        _resultsService = resultsService;
    }

    [HttpGet("/results/me")]
    public async Task<IActionResult> GetMyResult([FromQuery] int labNumber)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return Unauthorized(new { message = "Demo token is required." });
        }

        var result = await _resultsService.GetLatestStudentResultAsync(currentUser.UserId, NormalizeLabNumber(labNumber));
        if (result is null)
        {
            return NotFound(new { message = "Result not found." });
        }

        return Ok(result);
    }

    [HttpGet("/results/me/history")]
    public async Task<IActionResult> GetMyHistory([FromQuery] int labNumber)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return Unauthorized(new { message = "Demo token is required." });
        }

        var history = await _resultsService.GetStudentHistoryAsync(currentUser.UserId, NormalizeLabNumber(labNumber));
        if (history.Count == 0)
        {
            return NotFound(new { message = "Results history not found." });
        }

        return Ok(history);
    }

    [HttpGet("/results/students")]
    public async Task<IActionResult> GetStudentsResults([FromQuery] int labNumber)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return Unauthorized(new { message = "Demo token is required." });
        }

        if (!AccessService.IsStaffRole(currentUser.Role))
        {
            return Forbid();
        }

        var accessibleStudentIds = await _accessService.GetAccessibleStudentIdsAsync(currentUser);
        var results = await _resultsService.GetStudentsLatestResultsAsync(accessibleStudentIds, NormalizeLabNumber(labNumber));

        return Ok(results);
    }

    [HttpGet("/results/students/{studentId:int}")]
    public async Task<IActionResult> GetStudentResult(int studentId, [FromQuery] int labNumber)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return Unauthorized(new { message = "Demo token is required." });
        }

        if (!AccessService.IsStaffRole(currentUser.Role))
        {
            return Forbid();
        }

        if (!await _accessService.CanAccessStudentAsync(currentUser, studentId))
        {
            return Forbid();
        }

        var result = await _resultsService.GetLatestStudentResultAsync(studentId, NormalizeLabNumber(labNumber));
        if (result is null)
        {
            return NotFound(new { message = "Result not found." });
        }

        return Ok(result);
    }

    [HttpGet("/results/students/{studentId:int}/history")]
    public async Task<IActionResult> GetStudentHistory(int studentId, [FromQuery] int labNumber)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return Unauthorized(new { message = "Demo token is required." });
        }

        if (!AccessService.IsStaffRole(currentUser.Role))
        {
            return Forbid();
        }

        if (!await _accessService.CanAccessStudentAsync(currentUser, studentId))
        {
            return Forbid();
        }

        var history = await _resultsService.GetStudentHistoryAsync(studentId, NormalizeLabNumber(labNumber));
        if (history.Count == 0)
        {
            return NotFound(new { message = "Results history not found." });
        }

        return Ok(history);
    }

    public async Task<IActionResult> StudentHistory(int labNumber = 1)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, "Demo token is required.");
        }

        var normalizedLabNumber = NormalizeLabNumber(labNumber);
        var history = await _resultsService.GetStudentHistoryAsync(currentUser.UserId, normalizedLabNumber);

        var model = new StudentHistoryViewModel
        {
            CurrentUserName = currentUser.Name,
            StudentId = currentUser.UserId,
            StudentName = currentUser.Name,
            LabNumber = normalizedLabNumber,
            Token = currentUser.Token,
            IsOwnHistory = true,
            Items = history
                .Select(item => new StudentResultHistoryItemViewModel
                {
                    Status = item.Status,
                    Result = item.Result,
                    CheckedAt = item.CheckedAt
                })
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> StudentDetails(int studentId, int labNumber = 1)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, "Demo token is required.");
        }

        if (!AccessService.IsStaffRole(currentUser.Role))
        {
            return Forbid();
        }

        if (!await _accessService.CanAccessStudentAsync(currentUser, studentId))
        {
            return Forbid();
        }

        var normalizedLabNumber = NormalizeLabNumber(labNumber);
        var studentName = await _accessService.GetStudentNameAsync(studentId);
        var history = await _resultsService.GetStudentHistoryAsync(studentId, normalizedLabNumber);

        var model = new StudentHistoryViewModel
        {
            CurrentUserName = currentUser.Name,
            StudentId = studentId,
            StudentName = studentName,
            LabNumber = normalizedLabNumber,
            Token = currentUser.Token,
            IsOwnHistory = false,
            Items = history
                .Select(item => new StudentResultHistoryItemViewModel
                {
                    Status = item.Status,
                    Result = item.Result,
                    CheckedAt = item.CheckedAt
                })
                .ToList()
        };

        return View(model);
    }

    private static int NormalizeLabNumber(int labNumber)
    {
        return labNumber > 0 ? labNumber : 1;
    }
}
