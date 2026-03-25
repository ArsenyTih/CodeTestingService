using CodeTestingService.Models.Requests;
using CodeTestingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeTestingService.Controllers;

[ApiController]
[Route("code")]
public class CodeController : ControllerBase
{
    private readonly RepositoryService _repositoryService;

    public CodeController(RepositoryService repositoryService)
    {
        _repositoryService = repositoryService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdateRepository([FromBody] CreateRepositoryRequest request)
    {
        var repository = await _repositoryService.UpsertRepositoryAsync(
            request.StudentId,
            request.LabNumber,
            request.RepoUrl);

        return Accepted(new
        {
            message = "Repository binding accepted.",
            repository
        });
    }
}
