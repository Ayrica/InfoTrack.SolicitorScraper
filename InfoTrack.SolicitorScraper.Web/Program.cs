using InfoTrack.SolicitorScraper.Web;
using InfoTrack.SolicitorScraper.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";
var scrapeTimeoutMinutes = builder.Configuration.GetValue("ScrapeTimeoutMinutes", 30);

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

builder.Services.AddScoped<IScrapeApiClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ApiBaseUrl"] ?? apiBaseUrl;
    var timeoutMinutes = configuration.GetValue("ScrapeTimeoutMinutes", scrapeTimeoutMinutes);
    var scrapeClient = new HttpClient
    {
        BaseAddress = new Uri(baseUrl),
        Timeout = TimeSpan.FromMinutes(timeoutMinutes)
    };

    return new ScrapeApiClient(scrapeClient);
});

builder.Services.AddScoped<ILocationsApiClient, LocationsApiClient>();
builder.Services.AddScoped<ISolicitorsApiClient, SolicitorsApiClient>();
builder.Services.AddScoped<IReportsApiClient, ReportsApiClient>();

await builder.Build().RunAsync();