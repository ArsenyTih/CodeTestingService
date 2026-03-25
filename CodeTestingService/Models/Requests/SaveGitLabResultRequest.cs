using System.ComponentModel.DataAnnotations;

namespace CodeTestingService.Models.Requests;

public class SaveGitLabResultRequest
{
    [Required]
    public string RepoUrl { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;

    [Required]
    public string Result { get; set; } = string.Empty;

    public DateTime CheckedAt { get; set; }
}
