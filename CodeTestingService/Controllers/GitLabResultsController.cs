using CodeTestingService.Models.Requests;
using CodeTestingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeTestingService.Controllers;

[ApiController]
[Route("gitlab/results")]
public class GitLabResultsController : ControllerBase
{
    private readonly ResultsService _resultsService;

    public GitLabResultsController(ResultsService resultsService)
    {
        _resultsService = resultsService;
    }

    [HttpPost]
    public async Task<IActionResult> SaveResult([FromBody] SaveGitLabResultRequest request)
    {
        var savedResult = await _resultsService.SaveResultAsync(
            request.RepoUrl,
            request.Status,
            request.Result,
            request.CheckedAt);

        if (savedResult is null)
        {
            return NotFound(new { message = "Repository for specified repoUrl was not found." });
        }

        return Created(string.Empty, new
        {
            message = "Check result saved.",
            result = savedResult
        });
    }
}
