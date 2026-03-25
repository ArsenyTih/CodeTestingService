using CodeTestingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeTestingService.Controllers;

[ApiController]
[Route("repositories")]
public class RepositoriesController : ControllerBase
{
    private readonly DemoJwtUserService _demoJwtUserService;
    private readonly AccessService _accessService;
    private readonly RepositoryService _repositoryService;

    public RepositoriesController(
        DemoJwtUserService demoJwtUserService,
        AccessService accessService,
        RepositoryService repositoryService)
    {
        _demoJwtUserService = demoJwtUserService;
        _accessService = accessService;
        _repositoryService = repositoryService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyRepository([FromQuery] int labNumber)
    {
        var currentUser = await _demoJwtUserService.GetCurrentUserAsync(HttpContext);
        if (currentUser is null)
        {
            return Unauthorized(new { message = "Demo token is required." });
        }

        var repository = await _repositoryService.GetStudentRepositoryAsync(currentUser.UserId, NormalizeLabNumber(labNumber));
        if (repository is null)
        {
            return NotFound(new { message = "Repository not found." });
        }

        return Ok(repository);
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudentsRepositories([FromQuery] int labNumber)
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
        var repositories = await _repositoryService.GetStudentsRepositoriesAsync(accessibleStudentIds, NormalizeLabNumber(labNumber));

        return Ok(repositories);
    }

    [HttpGet("tests")]
    public async Task<IActionResult> GetTestsRepository([FromQuery] int labNumber)
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

        var repository = await _repositoryService.GetTestRepositoryAsync(NormalizeLabNumber(labNumber));
        if (repository is null)
        {
            return NotFound(new { message = "Test repository not found." });
        }

        return Ok(repository);
    }

    private static int NormalizeLabNumber(int labNumber)
    {
        return labNumber > 0 ? labNumber : 1;
    }
}
