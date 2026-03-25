namespace CodeTestingService.Models.ViewModels;

public class StudentPageViewModel
{
    public string CurrentUserName { get; set; } = string.Empty;

    public int StudentId { get; set; }

    public int LabNumber { get; set; }

    public string Token { get; set; } = string.Empty;

    public string? RepoUrl { get; set; }

    public List<LabSummaryItemViewModel> Labs { get; set; } = new();

    public List<ResultSelectionItemViewModel> Results { get; set; } = new();

    public int? SelectedResultId { get; set; }

    public string? SelectedResultStatus { get; set; }

    public string? SelectedResultContent { get; set; }

    public DateTime? SelectedResultCheckedAt { get; set; }
}

public class LabSummaryItemViewModel
{
    public int LabNumber { get; set; }

    public bool IsActive { get; set; }

    public string? Status { get; set; }

    public DateTime? CheckedAt { get; set; }
}

public class ResultSelectionItemViewModel
{
    public int Id { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CheckedAt { get; set; }

    public string Result { get; set; } = string.Empty;
}
