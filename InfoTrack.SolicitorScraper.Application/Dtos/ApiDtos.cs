namespace InfoTrack.SolicitorScraper.Application.Dtos;

public record UpdateLocationsRequest(IReadOnlyList<string> Locations);

public record ScrapeRequest(IReadOnlyList<string>? Locations);

public record SolicitorContactDto(

    Guid Id,
    string Name,
    string Location,
    string? Phone,
    string? Address,
    string? Description,
    string? WebsiteUrl,
    string? EmailUrl,
    string? ProfileUrl,
    int? ReviewCount,
    DateTime ScrapedDate);

public record ScrapeJobDto(
    Guid Id,
    IReadOnlyList<string> Locations,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    string? ErrorMessage,
    IReadOnlyList<string> FailedLocations,
    int TotalSolicitorsFound);

public record SolicitorReportSummaryDto(
    int TotalSolicitors,
    DateTime? LastScrapedDate,
    IReadOnlyList<LocationCountDto> ByLocation,
    IReadOnlyList<SolicitorContactDto> TopByReviewCount,
    IReadOnlyList<LocationCoverageDto> CoverageGaps,
    int SolicitorsWithoutPhone,
    int SolicitorsWithoutWebsite);

public record LocationCountDto(string Location, int Count);

public record LocationCoverageDto(string Location, int Count, bool HasResults);

public record ErrorResponse(string Error);