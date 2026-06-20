using System.Net.Http.Json;
using InfoTrack.SolicitorScraper.Web.Models;

namespace InfoTrack.SolicitorScraper.Web.Services;

public class SolicitorsApiClient : ISolicitorsApiClient
{
    private readonly HttpClient _httpClient;

    public SolicitorsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<SolicitorContact>> GetAsync(
        IReadOnlyList<string>? locations = null,
        CancellationToken cancellationToken = default)
    {
        var url = "api/solicitors";

        if (locations is { Count: > 0 })
        {
            var query = string.Join(',', locations);
            url += $"?locations={Uri.EscapeDataString(query)}";
        }

        var contacts = await _httpClient.GetFromJsonAsync<IReadOnlyList<SolicitorContact>>(url, cancellationToken);
        return contacts ?? [];
    }
}