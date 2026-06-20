namespace InfoTrack.SolicitorScraper.Web.Models;

public class ScrapeRequest
{
    public IReadOnlyList<string>? Locations { get; set; }
}

public class ScrapeJobResult
{
    public Guid Id { get; set; }
    public IReadOnlyList<string> Locations { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<string> FailedLocations { get; set; } = [];
    public int TotalSolicitorsFound { get; set; }
}
