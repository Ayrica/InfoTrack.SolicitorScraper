namespace InfoTrack.SolicitorScraper.Core.Entities;

public class SolicitorReportSummary
{
    public int TotalSolicitors { get; init; }
    public DateTime? LastScrapedDate { get; init; }
    public IReadOnlyList<LocationCount> ByLocation { get; init; } = [];
    public IReadOnlyList<SolicitorContact> TopByReviewCount { get; init; } = [];
    public IReadOnlyList<LocationCoverage> CoverageGaps { get; init; } = [];
    public int SolicitorsWithoutPhone { get; init; }
    public int SolicitorsWithoutWebsite { get; init; }
}

public class LocationCount
{
    public required string Location { get; init; }
    public int Count { get; init; }
}

public class LocationCoverage
{
    public required string Location { get; init; }
    public int Count { get; init; }
    public bool HasResults => Count > 0;
}