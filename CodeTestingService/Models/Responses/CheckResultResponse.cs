namespace CodeTestingService.Models.Responses;

public class CheckResultResponse
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int LabNumber { get; set; }

    public string RepoUrl { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public DateTime CheckedAt { get; set; }
}
