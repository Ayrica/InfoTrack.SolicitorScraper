using InfoTrack.SolicitorScraper.Application.Dtos;
using InfoTrack.SolicitorScraper.Core.Entities;

namespace InfoTrack.SolicitorScraper.Application.Mapping;

public static class DtoMapper
{
    public static SolicitorContactDto ToDto(this SolicitorContact contact) =>
        new(
            contact.Id,
            contact.Name,
            contact.Location,
            contact.Phone,
            contact.Address,
            contact.Description,
            contact.WebsiteUrl,
            contact.EmailUrl,
            contact.ProfileUrl,
            contact.ReviewCount,
            contact.ScrapedDate);

    public static ScrapeJobDto ToDto(this ScrapeJob job) =>
        new(
            job.Id,
            job.Locations,
            job.Status.ToString(),
            job.StartDate,
            job.EndDate,
            job.ErrorMessage,
            job.FailedLocations,
            job.TotalSolicitorsFound);

    public static SolicitorReportSummaryDto ToDto(this SolicitorReportSummary summary) =>
        new(
            summary.TotalSolicitors,
            summary.LastScrapedDate,
            summary.ByLocation.Select(x => new LocationCountDto(x.Location, x.Count)).ToList(),
            summary.TopByReviewCount.Select(ToDto).ToList(),
            summary.CoverageGaps.Select(x => new LocationCoverageDto(x.Location, x.Count, x.HasResults)).ToList(),
            summary.SolicitorsWithoutPhone,
            summary.SolicitorsWithoutWebsite);
}