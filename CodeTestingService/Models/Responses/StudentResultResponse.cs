namespace CodeTestingService.Models.Responses;

public class StudentResultResponse
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int LabNumber { get; set; }

    public string? RepoUrl { get; set; }

    public string? Status { get; set; }

    public string? Result { get; set; }

    public DateTime? CheckedAt { get; set; }
}
