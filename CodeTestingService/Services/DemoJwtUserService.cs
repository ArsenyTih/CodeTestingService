using CodeTestingService.Models.Demo;

namespace CodeTestingService.Services;

public class DemoJwtUserService
{
    private const string BearerPrefix = "Bearer ";

    private readonly DemoCoreDataService _demoCoreDataService;

    public DemoJwtUserService(DemoCoreDataService demoCoreDataService)
    {
        _demoCoreDataService = demoCoreDataService;
    }

    public async Task<CurrentUser?> GetCurrentUserAsync(HttpContext httpContext)
    {
        var token = ExtractToken(httpContext);
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var user = await _demoCoreDataService.GetUserByTokenAsync(token);
        if (user is null)
        {
            return null;
        }

        return new CurrentUser
        {
            UserId = user.UserId,
            Role = user.Role,
            Name = user.Name,
            Token = user.Token
        };
    }

    private static string? ExtractToken(HttpContext httpContext)
    {
        var authorizationHeader = httpContext.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorizationHeader) &&
            authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return authorizationHeader[BearerPrefix.Length..].Trim();
        }

        if (httpContext.Request.Query.TryGetValue("token", out var tokenValues))
        {
            return tokenValues.ToString();
        }

        return null;
    }
}
