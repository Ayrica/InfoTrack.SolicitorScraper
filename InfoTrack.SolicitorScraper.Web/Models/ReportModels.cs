namespace InfoTrack.SolicitorScraper.Web.Models;

public class SolicitorReportSummary
{
    public int TotalSolicitors { get; set; }
    public DateTime? LastScrapedDate { get; set; }
    public IReadOnlyList<LocationCount> ByLocation { get; set; } = [];
    public IReadOnlyList<SolicitorContact> TopByReviewCount { get; set; } = [];
    public IReadOnlyList<LocationCoverage> CoverageGaps { get; set; } = [];
    public int SolicitorsWithoutPhone { get; set; }
    public int SolicitorsWithoutWebsite { get; set; }
}

public class LocationCount
{
    public string Location { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class LocationCoverage
{
    public string Location { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool HasResults { get; set; }
}