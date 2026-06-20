using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfoTrack.SolicitorScraper.Infrastructure.Scraping;

public class SolicitorScraper : ISolicitorScraper
{
    private readonly SolicitorsPageClient _pageClient;
    private readonly SolicitorsHtmlParser _parser;
    private readonly ScrapingOptions _options;
    private readonly ILogger<SolicitorScraper> _logger;

    public SolicitorScraper(
        SolicitorsPageClient pageClient,
        SolicitorsHtmlParser parser,
        IOptions<ScrapingOptions> options,
        ILogger<SolicitorScraper> logger)
    {
        _pageClient = pageClient;
        _parser = parser;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SolicitorContact>> ScrapeLocationAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        if (_options.DelayBetweenRequestsMs > 0)
        {
            await Task.Delay(_options.DelayBetweenRequestsMs, cancellationToken);
        }

        var html = await _pageClient.FetchLocationPageAsync(location, cancellationToken);
        var contacts = _parser.Parse(html, location).ToList();
        await EnrichFromProfilePagesAsync(contacts, cancellationToken);

        _logger.LogInformation(
            "Parsed {Count} solicitor contacts for {Location}",
            contacts.Count,
            location);

        return contacts;
    }

    private async Task EnrichFromProfilePagesAsync(
        IList<SolicitorContact> contacts,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < contacts.Count; i++)
        {
            var contact = contacts[i];
            if (contact.ProfileUrl is null ||
                (contact.WebsiteUrl is not null && contact.EmailUrl is not null))
            {
                continue;
            }

            if (_options.DelayBetweenRequestsMs > 0)
            {
                await Task.Delay(_options.DelayBetweenRequestsMs, cancellationToken);
            }

            try
            {
                var profileHtml = await _pageClient.FetchPageAsync(contact.ProfileUrl, cancellationToken);
                var profileDetails = _parser.ParseProfileDetails(profileHtml);

                if (profileDetails.WebsiteUrl is null && profileDetails.EmailUrl is null)
                {
                    continue;
                }

                contacts[i] = MergeProfileDetails(contact, profileDetails);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to enrich solicitor {Name} from profile {ProfileUrl}",
                    contact.Name,
                    contact.ProfileUrl);
            }
        }
    }

    private static SolicitorContact MergeProfileDetails(
        SolicitorContact contact,
        ProfileContactDetails profileDetails) =>
        new()
        {
            Id = contact.Id,
            Name = contact.Name,
            Location = contact.Location,
            Phone = contact.Phone,
            Address = contact.Address,
            Description = contact.Description,
            WebsiteUrl = contact.WebsiteUrl ?? profileDetails.WebsiteUrl,
            EmailUrl = contact.EmailUrl ?? profileDetails.EmailUrl,
            ProfileUrl = contact.ProfileUrl,
            ReviewCount = contact.ReviewCount,
            ScrapedDate = contact.ScrapedDate
        };
}