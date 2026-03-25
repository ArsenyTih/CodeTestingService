namespace CodeTestingService.Models.ViewModels;

public class StudentResultHistoryItemViewModel
{
    public string Status { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public DateTime CheckedAt { get; set; }
}
