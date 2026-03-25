using System.ComponentModel.DataAnnotations;

namespace CodeTestingService.Models.Requests;

public class CreateRepositoryRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue)]
    public int LabNumber { get; set; }

    [Required]
    public string RepoUrl { get; set; } = string.Empty;
}
