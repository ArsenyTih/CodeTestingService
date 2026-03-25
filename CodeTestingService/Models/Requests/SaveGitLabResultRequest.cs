using System.ComponentModel.DataAnnotations;

namespace CodeTestingService.Models.Requests;

public class SaveGitLabResultRequest
{
    [Required]
    public string ProjectUrl { get; set; } = string.Empty;

    public string? ProjectId { get; set; }

    public string? PipelineId { get; set; }

    public string? MergeRequestIid { get; set; }

    public string? CommitSha { get; set; }

    [Required]
    public string Report { get; set; } = string.Empty;
}
