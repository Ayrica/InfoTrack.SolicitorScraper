using InfoTrack.SolicitorScraper.Application;
using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Infrastructure.Persistence;
using InfoTrack.SolicitorScraper.Infrastructure.Scraping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfoTrack.SolicitorScraper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ScrapingOptions>(configuration.GetSection(ScrapingOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("InfoTrackSolicitorScraper"));

        services.AddHttpClient<SolicitorsPageClient>((sp, client) =>
        {
            var options = configuration.GetSection(ScrapingOptions.SectionName).Get<ScrapingOptions>()
                ?? new ScrapingOptions();
            client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        });

        services.AddSingleton<SolicitorsHtmlParser>();
        services.AddScoped<ISolicitorScraper, Scraping.SolicitorScraper>();
        services.AddScoped<ISolicitorRepository, SolicitorRepository>();
        services.AddScoped<ILocationSettingsService, LocationSettingsService>();

        services.AddApplication();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var locationSettings = scope.ServiceProvider.GetRequiredService<ILocationSettingsService>();
        await locationSettings.SeedDefaultLocationsIfEmptyAsync();
    }
}
