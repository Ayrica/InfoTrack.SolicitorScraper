using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InfoTrack.SolicitorScraper.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IScrapeOrchestrator, ScrapeOrchestrator>();
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}