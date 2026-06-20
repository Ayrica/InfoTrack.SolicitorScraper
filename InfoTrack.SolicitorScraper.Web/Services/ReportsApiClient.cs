using System.Net.Http.Json;
using InfoTrack.SolicitorScraper.Web.Models;

namespace InfoTrack.SolicitorScraper.Web.Services;

public class ReportsApiClient : IReportsApiClient
{
    private readonly HttpClient _httpClient;

    public ReportsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SolicitorReportSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var summary = await _httpClient.GetFromJsonAsync<SolicitorReportSummary>("api/reports/summary", cancellationToken);
        return summary ?? new SolicitorReportSummary();
    }
}