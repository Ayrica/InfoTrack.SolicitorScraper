using InfoTrack.SolicitorScraper.Web.Models;

namespace InfoTrack.SolicitorScraper.Web.Services;

public interface IReportsApiClient
{
    Task<SolicitorReportSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
}