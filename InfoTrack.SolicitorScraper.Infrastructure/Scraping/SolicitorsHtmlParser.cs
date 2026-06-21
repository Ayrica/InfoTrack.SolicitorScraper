using System.Net;
using System.Text.RegularExpressions;
using InfoTrack.SolicitorScraper.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfoTrack.SolicitorScraper.Infrastructure.Scraping;

public partial class SolicitorsHtmlParser
{
    private readonly ILogger<SolicitorsHtmlParser> _logger;
    private readonly string _siteBaseUrl;

    public SolicitorsHtmlParser(
        ILogger<SolicitorsHtmlParser> logger,
        IOptions<ScrapingOptions> options)
    {
        _logger = logger;
        _siteBaseUrl = options.Value.SiteBaseUrl.TrimEnd('/');
    }

    public IReadOnlyList<SolicitorContact> Parse(string html, string location)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return [];
        }

        var sectionStart = html.IndexOf("class=\"result-section\"", StringComparison.OrdinalIgnoreCase);
        if (sectionStart < 0)
        {
            _logger.LogWarning("No result-section found for location {Location}", location);
            return [];
        }

        var sectionHtml = html[sectionStart..];
        var blocks = ExtractResultBlocks(sectionHtml);
        var results = new List<SolicitorContact>(blocks.Count);

        foreach (var block in blocks)
        {
            var contact = ParseResultItem(block, location);
            if (contact is not null)
            {
                results.Add(contact);
            }
        }

        _logger.LogInformation(
            "Parsed {Count} solicitor contacts from {BlockCount} result blocks for {Location}",
            results.Count,
            blocks.Count,
            location);

        return results;
    }

    public ProfileContactDetails ParseProfileDetails(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return ProfileContactDetails.Empty;
        }

        return new ProfileContactDetails(
            ExtractProfileWebsite(html),
            ExtractProfileEmailUrl(html));
    }

    private static List<string> ExtractResultBlocks(string sectionHtml)
    {
        var blocks = new List<string>();
        var matches = ResultItemOpenPattern().Matches(sectionHtml);

        for (var i = 0; i < matches.Count; i++)
        {
            var start = matches[i].Index + matches[i].Length;
            var end = i + 1 < matches.Count
                ? matches[i + 1].Index
                : sectionHtml.IndexOf("<div class=\"banner-block\"", start, StringComparison.OrdinalIgnoreCase);

            if (end < 0)
            {
                end = sectionHtml.Length;
            }

            blocks.Add(sectionHtml[start..end]);
        }

        return blocks;
    }

    private SolicitorContact? ParseResultItem(string block, string location)
    {
        var name = ExtractName(block);
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new SolicitorContact
        {
            Name = WebUtility.HtmlDecode(name.Trim()),
            Location = location,
            Phone = ExtractPhone(block),
            Address = ExtractAddress(block),
            Description = ExtractDescription(block),
            WebsiteUrl = ExtractWebsite(block),
            EmailUrl = ExtractEmailUrl(block),
            ProfileUrl = ExtractProfileUrl(block),
            ReviewCount = ExtractReviewCount(block),
            ScrapedDate = DateTime.UtcNow
        };
    }

    private static string? ExtractName(string block)
    {
        var match = NamePattern().Match(block);
        return match.Success ? StripTags(match.Groups[1].Value) : null;
    }

    private static string? ExtractPhone(string block)
    {
        var phoneBlock = PhoneBlockPattern().Match(block);
        if (phoneBlock.Success)
        {
            var displayText = StripTags(phoneBlock.Groups[2].Value).Trim();
            return string.IsNullOrWhiteSpace(displayText)
                ? StripTags(phoneBlock.Groups[1].Value).Trim()
                : displayText;
        }

        var telLink = TelClassPhonePattern().Match(block);
        if (telLink.Success)
        {
            var displayText = StripTags(telLink.Groups[2].Value).Trim();
            return string.IsNullOrWhiteSpace(displayText)
                ? StripTags(telLink.Groups[1].Value).Trim()
                : displayText;
        }

        return null;
    }

    private static string? ExtractAddress(string block)
    {
        var match = AddressPattern().Match(block);
        return match.Success ? WebUtility.HtmlDecode(StripTags(match.Groups[1].Value).Trim()) : null;
    }

    private static string? ExtractDescription(string block)
    {
        var match = DescriptionPattern().Match(block);
        return match.Success ? WebUtility.HtmlDecode(StripTags(match.Groups[1].Value).Trim()) : null;
    }

    private static string? ExtractWebsite(string block)
    {
        var match = WebsitePattern().Match(block);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ExtractProfileWebsite(string html)
    {
        var match = ProfileWebsitePattern().Match(html);
        if (!match.Success)
        {
            return null;
        }

        return match.Groups[1].Success
            ? match.Groups[1].Value.Trim()
            : match.Groups[2].Value.Trim();
    }

    private string? ExtractProfileEmailUrl(string html)
    {
        var match = ProfileEmailPattern().Match(html);
        if (!match.Success)
        {
            return null;
        }

        return ResolveAbsoluteUrl(match.Groups[1].Value.Trim());
    }

    private string? ExtractEmailUrl(string block)
    {
        var match = EmailPattern().Match(block);
        if (!match.Success)
        {
            return null;
        }

        return ResolveAbsoluteUrl(match.Groups[1].Value.Trim());
    }

    private string? ExtractProfileUrl(string block)
    {
        var match = ProfilePattern().Match(block);
        if (!match.Success)
        {
            return null;
        }

        var path = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
        return ResolveAbsoluteUrl(path.Trim());
    }

    private string ResolveAbsoluteUrl(string path) =>
        path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? path
            : $"{_siteBaseUrl}{path}";

    private static int? ExtractReviewCount(string block)
    {
        var match = ReviewCountPattern().Match(block);
        return match.Success && int.TryParse(match.Groups[1].Value, out var count) ? count : null;
    }

    private static string StripTags(string value) =>
        TagPattern().Replace(value, string.Empty).Trim();

    [GeneratedRegex(@"<div\s+class=""result-item(?:\s+item-small)?"">", RegexOptions.IgnoreCase)]
    private static partial Regex ResultItemOpenPattern();

    [GeneratedRegex(@"<span\s+class=""h2"">([^<]+)", RegexOptions.IgnoreCase)]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"href=""tel:([^""]+)""[^>]*>([^<]*)</a>", RegexOptions.IgnoreCase)]
    private static partial Regex PhoneBlockPattern();

    [GeneratedRegex(@"<a\s+class=""tel""[^>]*href=""tel:([^""]+)""[^>]*>([^<]*)</a>", RegexOptions.IgnoreCase)]
    private static partial Regex TelClassPhonePattern();

    [GeneratedRegex(@"<address>(.*?)</address>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AddressPattern();

    [GeneratedRegex(@"</address>\s*</a>\s*(?:<a\s+class=""tel""[^>]*>.*?</a>\s*)?<p>(.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex DescriptionPattern();

    [GeneratedRegex(@"<li><a[^>]+href=""([^""]+)""[^>]*>[\s\S]*?fa-globe[\s\S]*?Website</a></li>", RegexOptions.IgnoreCase)]
    private static partial Regex WebsitePattern();

    [GeneratedRegex(@"href=""(https?://[^""]+)""[^>]*class=""website""|class=""website""[^>]*href=""(https?://[^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex ProfileWebsitePattern();

    [GeneratedRegex(@"<li[^>]*><a[^>]+href=""(/enquiry-form[^""]*)""[^>]*>[\s\S]*?fa-envelope[\s\S]*?Email</a></li>", RegexOptions.IgnoreCase)]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"href=""(/enquiry-form\.asp\?SiD=[^""]*)""[^>]*>[\s\S]*?fa-envelope", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ProfileEmailPattern();

    [GeneratedRegex(@"href=""([^""]+)""[^>]*class=""link-map""|class=""link-map""[^>]*href=""([^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex ProfilePattern();

    [GeneratedRegex(@"rev-results""[^>]*>.*?\((\d+)\)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ReviewCountPattern();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex TagPattern();
}