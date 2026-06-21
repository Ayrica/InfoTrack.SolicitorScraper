using InfoTrack.SolicitorScraper.Infrastructure.Scraping;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace InfoTrack.SolicitorScraper.Tests;

public class SolicitorsHtmlParserTests
{
    private readonly SolicitorsHtmlParser _parser = new(
        NullLogger<SolicitorsHtmlParser>.Instance,
        Options.Create(new ScrapingOptions()));

    [Fact]
    public void Parse_ExtractsContactDetailsFromResultSection()
    {
        var html = """
            <div class="result-section">
                <div class="heading">
                    <h5 class="h1 text-center">Search <strong>Results</strong></h5>
                </div>
                <div class="result-item">
                    <div class="top-holder">
                        <span class="h2">Hanne &amp; Co Solicitors LLP<span class="rev-results">(560)</span></span>
                        <div class="phone-block mobile-hidden">
                            <span>Phone:</span>
                            <a rel="noindex" href="tel:02072280017">020 7228 0017</a>
                        </div>
                    </div>
                    <a href="/hanne-and-co-solicitors-llp.html" class="link-map"><i class="fa fa-map-marker"></i><address>3 Calico House, Battersea, London SW11 3TN</address></a>
                    <p>We provide a unique residential property service.</p>
                    <ul class="list-item">
                        <li class="red-color"><a rel="noindex nofollow" href="/enquiry-form.asp?SiD=NTYwMzM"><i class="fa fa-envelope"></i>Email</a></li>
                        <li><a target="_blank" href="http://www.hanne.co.uk" rel="nofollow"><i class="fa fa-globe"></i>Website</a></li>
                    </ul>
                </div>
            </div>
            """;

        var results = _parser.Parse(html, "London");

        Assert.Single(results);
        var contact = results[0];
        Assert.Equal("Hanne & Co Solicitors LLP", contact.Name);
        Assert.Equal("London", contact.Location);
        Assert.Equal("020 7228 0017", contact.Phone);
        Assert.Contains("Calico House", contact.Address);
        Assert.Equal(560, contact.ReviewCount);
        Assert.Equal("http://www.hanne.co.uk", contact.WebsiteUrl);
        Assert.StartsWith("https://www.solicitors.com/enquiry-form", contact.EmailUrl);
        Assert.Equal("https://www.solicitors.com/hanne-and-co-solicitors-llp.html", contact.ProfileUrl);
    }

    [Fact]
    public void Parse_ExtractsCompactItemSmallListings()
    {
        var html = """
            <div class="result-section">
                <div class="result-item item-small">
                    <span class="h2">Freeths LLP<span class="rev-results">(157)</span></span>
                    <a href="/freeths-llp.html" class="link-map"><i class="fa fa-map-marker"></i><address>3rd Floor Core 3 Exchange Station, Liverpool, L2 2QP</address></a>
                    <a class="tel" rel="noindex" href="tel:+44(0)3450094425">+44 (0)345 009 4425</a>
                    <p>Specialist property advice.</p>
                </div>
            </div>
            """;

        var results = _parser.Parse(html, "Liverpool");

        Assert.Single(results);
        var contact = results[0];
        Assert.Equal("Freeths LLP", contact.Name);
        Assert.Equal("+44 (0)345 009 4425", contact.Phone);
        Assert.Equal(157, contact.ReviewCount);
        Assert.Equal("https://www.solicitors.com/freeths-llp.html", contact.ProfileUrl);
        Assert.Null(contact.WebsiteUrl);
    }

    [Fact]
    public void Parse_LiverpoolFixture_ReturnsFeaturedAndCompactListings()
    {
        var html = File.ReadAllText(Path.Combine("TestData", "liverpool-snippet.html"));
        var results = _parser.Parse(html, "Liverpool");

        Assert.True(results.Count >= 4);
        Assert.Contains(results, r => r.Name == "Kirwans");
        Assert.Contains(results, r => r.Name == "QualitySolicitors");
        Assert.Contains(results, r => r.Name == "Freeths LLP");
        Assert.Contains(results, r => r.Name == "Bartletts Solicitors");
    }

    [Fact]
    public void Parse_ReturnsEmpty_WhenResultSectionMissing()
    {
        var results = _parser.Parse("<html><body>No results here</body></html>", "London");
        Assert.Empty(results);
    }

    [Fact]
    public void ParseProfileDetails_ExtractsWebsiteAndEmailFromProfilePage()
    {
        var html = """
            <div class="links-holder">
                <a href="tel:+44 (0) 151 294 4722" class="phone"><i class="fa fa-phone"></i> Phone</a>
                <a href="https://ai-law.co.uk/" target="_blank" rel="nofollow" class="website"><i class="fa fa-globe"></i> Website: Ai Law</a>
                <a rel="noindex nofollow" href="/enquiry-form.asp?SiD=NzgyNjk&DiD="><i class="fa fa-envelope" aria-hidden="true"></i> Email</a>
            </div>
            """;

        var details = _parser.ParseProfileDetails(html);

        Assert.Equal("https://ai-law.co.uk/", details.WebsiteUrl);
        Assert.Equal("https://www.solicitors.com/enquiry-form.asp?SiD=NzgyNjk&DiD=", details.EmailUrl);
    }
}