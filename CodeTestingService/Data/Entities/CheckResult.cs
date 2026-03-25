namespace CodeTestingService.Data.Entities;

public class CheckResult
{
    public int Id { get; set; }

    public int RepositoryId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public DateTime CheckedAt { get; set; }

    public Repository Repository { get; set; } = null!;
}
