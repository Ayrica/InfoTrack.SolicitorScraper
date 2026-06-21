namespace InfoTrack.SolicitorScraper.Infrastructure.Scraping;

public class ScrapingOptions
{
    public const string SectionName = "Scraping";
    public string SiteBaseUrl { get; set; } = "https://www.solicitors.com";
    public string TemplateBaseUrl { get; set; } = "https://www.solicitors.com/conveyancing+{location}.html";
    public int RequestTimeoutSeconds { get; set; } = 30;
    public int DelayBetweenRequestsMs { get; set; } = 500;
    public string UserAgent { get; set; } = "InfoTrack-SolicitorScraper/1.0";
}