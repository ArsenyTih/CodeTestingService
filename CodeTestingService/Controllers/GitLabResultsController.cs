using System.Net.Http.Headers;
using CodeTestingService.Models.Requests;
using CodeTestingService.Models.Options;
using CodeTestingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CodeTestingService.Controllers;

[ApiController]
[Route("api/gitlab/results")]
public class GitLabResultsController : ControllerBase
{
    private readonly ResultsService _resultsService;
    private readonly GitLabResultsOptions _options;
    private readonly ILogger<GitLabResultsController> _logger;

    public GitLabResultsController(
        ResultsService resultsService,
        IOptions<GitLabResultsOptions> options,
        ILogger<GitLabResultsController> logger)
    {
        _resultsService = resultsService;
        _options = options.Value;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SaveResult([FromBody] SaveGitLabResultRequest request)
    {
        if (!TryGetBearerToken(out var bearerToken) ||
            string.IsNullOrWhiteSpace(_options.ServiceApiToken) ||
            !string.Equals(bearerToken, _options.ServiceApiToken, StringComparison.Ordinal))
        {
            return Unauthorized(new { message = "Invalid service API token." });
        }

        if (string.IsNullOrWhiteSpace(request.ProjectUrl) || string.IsNullOrWhiteSpace(request.Report))
        {
            return BadRequest(new { message = "Fields 'projectUrl' and 'report' are required." });
        }

        var savedResult = await _resultsService.SaveGitLabResultAsync(
            request.ProjectUrl,
            request.Report);

        if (savedResult is null)
        {
            _logger.LogWarning(
                "GitLab result was rejected because repository was not found for projectUrl {ProjectUrl}.",
                request.ProjectUrl);
            return BadRequest(new { message = "Repository for specified projectUrl was not found." });
        }

        return Ok(new
        {
            message = "GitLab result saved.",
            result = savedResult
        });
    }

    private bool TryGetBearerToken(out string token)
    {
        token = string.Empty;

        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValues))
        {
            return false;
        }

        if (!AuthenticationHeaderValue.TryParse(authorizationHeaderValues.ToString(), out var headerValue) ||
            !string.Equals(headerValue.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(headerValue.Parameter))
        {
            return false;
        }

        token = headerValue.Parameter;
        return true;
    }
}
