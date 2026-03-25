namespace CodeTestingService.Models.ViewModels;

public class TeacherPageViewModel
{
    public string CurrentUserName { get; set; } = string.Empty;

    public string CurrentUserRole { get; set; } = string.Empty;

    public int LabNumber { get; set; }

    public string Token { get; set; } = string.Empty;

    public string? TestRepositoryUrl { get; set; }

    public List<TeacherStudentItemViewModel> Students { get; set; } = new();
}

public class TeacherStudentItemViewModel
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string? RepoUrl { get; set; }

    public string? Status { get; set; }

    public string? Result { get; set; }

    public DateTime? CheckedAt { get; set; }
}
