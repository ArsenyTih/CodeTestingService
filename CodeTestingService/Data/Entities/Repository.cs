namespace CodeTestingService.Data.Entities;

public class Repository
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int LabNumber { get; set; }

    public string RepoUrl { get; set; } = string.Empty;

    public List<CheckResult> CheckResults { get; set; } = new();
}
