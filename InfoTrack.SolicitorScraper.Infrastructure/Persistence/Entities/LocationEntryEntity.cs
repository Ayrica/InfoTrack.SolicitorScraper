namespace InfoTrack.SolicitorScraper.Infrastructure.Persistence.Entities;

public class LocationEntryEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}