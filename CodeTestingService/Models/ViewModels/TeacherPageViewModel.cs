namespace CodeTestingService.Models.ViewModels;

public class TeacherPageViewModel
{
    public string CurrentUserName { get; set; } = string.Empty;

    public string CurrentUserRole { get; set; } = string.Empty;

    public string ActionName { get; set; } = string.Empty;

    public int LabNumber { get; set; }

    public string Token { get; set; } = string.Empty;

    public string? TestRepositoryUrl { get; set; }

    public List<LabOptionItemViewModel> Labs { get; set; } = new();

    public List<TeacherStudentItemViewModel> Students { get; set; } = new();

    public int? SelectedStudentId { get; set; }

    public string? SelectedStudentName { get; set; }

    public string? SelectedStudentRepoUrl { get; set; }

    public List<ResultSelectionItemViewModel> Results { get; set; } = new();

    public int? SelectedResultId { get; set; }

    public string? SelectedResultStatus { get; set; }

    public string? SelectedResultContent { get; set; }

    public DateTime? SelectedResultCheckedAt { get; set; }
}

public class TeacherStudentItemViewModel
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string? RepoUrl { get; set; }

    public string? Status { get; set; }

    public DateTime? CheckedAt { get; set; }

    public bool IsActive { get; set; }
}

public class LabOptionItemViewModel
{
    public int LabNumber { get; set; }

    public bool IsActive { get; set; }
}
