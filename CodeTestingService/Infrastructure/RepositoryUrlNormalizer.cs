namespace CodeTestingService.Infrastructure;

public static class RepositoryUrlNormalizer
{
    public static string Normalize(string repoUrl)
    {
        return string.IsNullOrWhiteSpace(repoUrl)
            ? string.Empty
            : repoUrl.Trim().TrimEnd('/');
    }
}
