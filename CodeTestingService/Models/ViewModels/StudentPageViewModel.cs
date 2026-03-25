namespace CodeTestingService.Models.ViewModels;

public class StudentPageViewModel
{
    public string CurrentUserName { get; set; } = string.Empty;

    public int StudentId { get; set; }

    public int LabNumber { get; set; }

    public string Token { get; set; } = string.Empty;

    public string? RepoUrl { get; set; }

    public string? Status { get; set; }

    public string? Result { get; set; }

    public DateTime? CheckedAt { get; set; }
}
