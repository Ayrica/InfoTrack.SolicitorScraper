namespace InfoTrack.SolicitorScraper.Web.Models;

public class UpdateLocationsRequest
{
    public IReadOnlyList<string> Locations { get; set; } = [];
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}