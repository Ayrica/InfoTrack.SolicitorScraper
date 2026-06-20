namespace InfoTrack.SolicitorScraper.Core.Entities;

public class ScrapeJob
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required IReadOnlyList<string> Locations { get; init; }
    public ScrapeJobStatus Status { get; set; }
    public DateTime StartDate { get; init; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<string> FailedLocations { get; set; } = [];
    public int TotalSolicitorsFound { get; set; }
}