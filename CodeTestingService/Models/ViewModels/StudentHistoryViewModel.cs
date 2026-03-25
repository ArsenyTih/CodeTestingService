namespace CodeTestingService.Models.ViewModels;

public class StudentHistoryViewModel
{
    public string CurrentUserName { get; set; } = string.Empty;

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int LabNumber { get; set; }

    public string Token { get; set; } = string.Empty;

    public bool IsOwnHistory { get; set; }

    public List<StudentResultHistoryItemViewModel> Items { get; set; } = new();
}
