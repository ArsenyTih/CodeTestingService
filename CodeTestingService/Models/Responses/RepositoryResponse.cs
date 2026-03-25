namespace CodeTestingService.Models.Responses;

public class RepositoryResponse
{
    public int StudentId { get; set; }

    public int LabNumber { get; set; }

    public string RepoUrl { get; set; } = string.Empty;
}
