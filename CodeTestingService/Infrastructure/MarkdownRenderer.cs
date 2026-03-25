using Markdig;

namespace CodeTestingService.Infrastructure;

public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public static string ToHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return "<p class=\"empty-state\">Результат проверки отсутствует.</p>";
        }

        return Markdown.ToHtml(markdown, Pipeline);
    }
}
