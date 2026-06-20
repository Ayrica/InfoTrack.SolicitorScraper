namespace InfoTrack.SolicitorScraper.Infrastructure.Scraping;

public record ProfileContactDetails(string? WebsiteUrl, string? EmailUrl)
{
    public static ProfileContactDetails Empty { get; } = new(null, null);
}