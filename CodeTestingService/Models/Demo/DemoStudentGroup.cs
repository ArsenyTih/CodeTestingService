namespace CodeTestingService.Models.Demo;

public class DemoStudentGroup
{
    public int? TeacherId { get; set; }

    public int? AssistantId { get; set; }

    public List<int> StudentIds { get; set; } = new();
}
