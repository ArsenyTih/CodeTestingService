namespace CodeTestingService.Models.Responses;

public class StudentRepositoryResponse
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int LabNumber { get; set; }

    public string? RepoUrl { get; set; }
}
