using System.Net.Http.Json;
using InfoTrack.SolicitorScraper.Web.Models;

namespace InfoTrack.SolicitorScraper.Web.Services;

public class ScrapeApiClient : IScrapeApiClient
{
    private readonly HttpClient _httpClient;

    public ScrapeApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ScrapeJobResult> ScrapeAsync(
        IReadOnlyList<string> locations,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/scrape",
            new ScrapeRequest { Locations = locations },
            cancellationToken);

        await ApiResponseHelper.EnsureSuccessAsync(response, cancellationToken);

        var job = await response.Content.ReadFromJsonAsync<ScrapeJobResult>(cancellationToken);
        return job ?? throw new ApiException("Scrape completed but returned no result.");
    }
}
