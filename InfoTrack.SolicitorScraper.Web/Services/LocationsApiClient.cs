using System.Net.Http.Json;
using InfoTrack.SolicitorScraper.Web.Models;

namespace InfoTrack.SolicitorScraper.Web.Services;

public class LocationsApiClient : ILocationsApiClient
{
    private readonly HttpClient _httpClient;

    public LocationsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<string>> GetAsync(CancellationToken cancellationToken = default)
    {
        var locations = await _httpClient.GetFromJsonAsync<IReadOnlyList<string>>("api/locations", cancellationToken);
        return locations ?? [];
    }

    public async Task<IReadOnlyList<string>> UpdateAsync(
        IReadOnlyList<string> locations,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            "api/locations",
            new UpdateLocationsRequest { Locations = locations },
            cancellationToken);

        await ApiResponseHelper.EnsureSuccessAsync(response, cancellationToken);

        var updated = await response.Content.ReadFromJsonAsync<IReadOnlyList<string>>(cancellationToken);
        return updated ?? [];
    }
}