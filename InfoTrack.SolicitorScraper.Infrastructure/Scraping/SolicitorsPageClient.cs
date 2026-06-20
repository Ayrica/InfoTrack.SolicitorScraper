using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfoTrack.SolicitorScraper.Infrastructure.Scraping;

public class SolicitorsPageClient
{
    private readonly HttpClient _httpClient;
    private readonly ScrapingOptions _options;
    private readonly ILogger<SolicitorsPageClient> _logger;

    public SolicitorsPageClient(
        HttpClient httpClient,
        IOptions<ScrapingOptions> options,
        ILogger<SolicitorsPageClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> FetchLocationPageAsync(string location, CancellationToken cancellationToken = default)
    {
        var slug = NormalizeLocationSlug(location);
        var url = _options.BaseUrlTemplate.Replace("{location}", slug, StringComparison.OrdinalIgnoreCase);
        return await FetchPageAsync(url, cancellationToken);
    }

    public async Task<string> FetchPageAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching page from {Url}", url);

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public static string NormalizeLocationSlug(string location) =>
        location.Trim().ToLowerInvariant().Replace(" ", string.Empty, StringComparison.Ordinal);
}