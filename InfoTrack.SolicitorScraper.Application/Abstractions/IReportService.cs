using InfoTrack.SolicitorScraper.Core.Entities;

namespace InfoTrack.SolicitorScraper.Application.Abstractions;

public interface IReportService
{
    Task<SolicitorReportSummary> BuildSummaryAsync(CancellationToken cancellationToken = default);
}