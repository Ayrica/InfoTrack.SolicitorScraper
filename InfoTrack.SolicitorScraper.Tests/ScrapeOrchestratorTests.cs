using InfoTrack.SolicitorScraper.Application.Abstractions;
using InfoTrack.SolicitorScraper.Application.Services;
using InfoTrack.SolicitorScraper.Core.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace InfoTrack.SolicitorScraper.Tests;

public class ScrapeOrchestratorTests
{
    [Fact]
    public async Task RunAsync_Throws_WhenNoLocationsConfigured()
    {
        var orchestrator = CreateOrchestrator(
            scraper: FakeScraper.Returns(1),
            configuredLocations: []);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orchestrator.RunAsync(null));

        Assert.Equal("No locations configured for scraping.", exception.Message);
    }

    [Fact]
    public async Task RunAsync_UsesConfiguredLocations_WhenLocationsArgumentIsNull()
    {
        var repository = new FakeSolicitorRepository();
        var orchestrator = CreateOrchestrator(
            scraper: FakeScraper.Returns(2),
            repository: repository,
            configuredLocations: ["London", "Liverpool"]);

        var job = await orchestrator.RunAsync(null);

        Assert.Equal(ScrapeJobStatus.Completed, job.Status);
        Assert.Equal(["London", "Liverpool"], job.Locations);
        Assert.Equal(4, job.TotalSolicitorsFound);
        Assert.Equal(2, repository.ReplaceCallCount);
    }

    [Fact]
    public async Task RunAsync_ReturnsCompleted_WhenAllLocationsSucceed()
    {
        var repository = new FakeSolicitorRepository();
        var orchestrator = CreateOrchestrator(
            scraper: FakeScraper.Returns(3),
            repository: repository);

        var job = await orchestrator.RunAsync(["London", "Manchester"]);

        Assert.Equal(ScrapeJobStatus.Completed, job.Status);
        Assert.NotEqual(ScrapeJobStatus.Running, job.Status);
        Assert.Empty(job.FailedLocations);
        Assert.Null(job.ErrorMessage);
        Assert.Equal(6, job.TotalSolicitorsFound);
        Assert.NotNull(job.EndDate);
        Assert.Equal(2, repository.ReplaceCallCount);
    }

    [Fact]
    public async Task RunAsync_ReturnsFailed_WhenAllLocationsFail()
    {
        var repository = new FakeSolicitorRepository();
        var orchestrator = CreateOrchestrator(
            scraper: FakeScraper.ThrowsFor("London", "Liverpool"),
            repository: repository);

        var job = await orchestrator.RunAsync(["London", "Liverpool"]);

        Assert.Equal(ScrapeJobStatus.Failed, job.Status);
        Assert.Equal(["London", "Liverpool"], job.FailedLocations);
        Assert.Equal("All locations failed to scrape.", job.ErrorMessage);
        Assert.Equal(0, job.TotalSolicitorsFound);
        Assert.Equal(0, repository.ReplaceCallCount);
    }

    [Fact]
    public async Task RunAsync_ReturnsPartiallyCompleted_WhenSomeLocationsFail()
    {
        var repository = new FakeSolicitorRepository();
        var orchestrator = CreateOrchestrator(
            scraper: FakeScraper.ThrowsFor("Liverpool"),
            repository: repository);

        var job = await orchestrator.RunAsync(["London", "Liverpool", "Bristol"]);

        Assert.Equal(ScrapeJobStatus.PartiallyCompleted, job.Status);
        Assert.Equal(["Liverpool"], job.FailedLocations);
        Assert.Null(job.ErrorMessage);
        Assert.Equal(2, job.TotalSolicitorsFound);
        Assert.Equal(2, repository.ReplaceCallCount);
    }

    [Fact]
    public async Task RunAsync_EntersRunningStateWhileProcessingLocations()
    {
        var observedRunning = false;
        var scraper = new FakeScraper(async (location, cancellationToken) =>
        {
            if (location == "London")
            {
                observedRunning = true;
            }

            await Task.Delay(10, cancellationToken);
            return [CreateContact(location)];
        });

        var orchestrator = CreateOrchestrator(scraper: scraper);

        var job = await orchestrator.RunAsync(["London"]);

        Assert.True(observedRunning);
        Assert.Equal(ScrapeJobStatus.Completed, job.Status);
    }

    [Fact]
    public async Task RunAsync_PersistsContactsInsideTransaction()
    {
        var unitOfWork = new FakeUnitOfWork();
        var repository = new FakeSolicitorRepository();
        var orchestrator = CreateOrchestrator(
            scraper: FakeScraper.Returns(1),
            repository: repository,
            unitOfWork: unitOfWork);

        await orchestrator.RunAsync(["London"]);

        Assert.Equal(1, unitOfWork.TransactionCount);
        Assert.Equal(1, repository.ReplaceCallCount);
    }

    private static ScrapeOrchestrator CreateOrchestrator(
        ISolicitorScraper scraper,
        FakeSolicitorRepository? repository = null,
        FakeUnitOfWork? unitOfWork = null,
        IReadOnlyList<string>? configuredLocations = null)
    {
        repository ??= new FakeSolicitorRepository();
        unitOfWork ??= new FakeUnitOfWork();
        var locationSettings = new FakeLocationSettingsService(
            configuredLocations ?? ["London", "Manchester"]);

        return new ScrapeOrchestrator(
            scraper,
            repository,
            unitOfWork,
            locationSettings,
            NullLogger<ScrapeOrchestrator>.Instance);
    }

    private static SolicitorContact CreateContact(string location) =>
        new()
        {
            Name = $"{location} Solicitors",
            Location = location
        };

    private class FakeScraper : ISolicitorScraper
    {
        private readonly Func<string, CancellationToken, Task<IReadOnlyList<SolicitorContact>>> _handler;

        public FakeScraper(Func<string, CancellationToken, Task<IReadOnlyList<SolicitorContact>>> handler)
        {
            _handler = handler;
        }

        public static FakeScraper Returns(int countPerLocation) =>
            new((location, _) => Task.FromResult<IReadOnlyList<SolicitorContact>>(
                Enumerable.Range(0, countPerLocation)
                    .Select(i => new SolicitorContact
                    {
                        Name = $"{location} Contact {i + 1}",
                        Location = location
                    })
                    .ToList()));

        public static FakeScraper ThrowsFor(params string[] locations)
        {
            var failing = new HashSet<string>(locations, StringComparer.OrdinalIgnoreCase);
            return new((location, _) => failing.Contains(location)
                ? throw new HttpRequestException($"Scrape failed for {location}")
                : Returns(1)._handler(location, CancellationToken.None));
        }

        public Task<IReadOnlyList<SolicitorContact>> ScrapeLocationAsync(
            string location,
            CancellationToken cancellationToken) =>
            _handler(location, cancellationToken);
    }

    private class FakeSolicitorRepository : ISolicitorRepository
    {
        public int ReplaceCallCount { get; private set; }

        public Task ReplaceForLocationAsync(
            string location,
            IReadOnlyList<SolicitorContact> contacts,
            CancellationToken cancellationToken = default)
        {
            ReplaceCallCount++;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SolicitorContact>> GetByLocationsAsync(
            IReadOnlyList<string>? locations,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SolicitorContact>>([]);

        public Task DeleteExceptLocationsAsync(
            IReadOnlyList<string> locationsToKeep,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public int TransactionCount { get; private set; }

        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            TransactionCount++;
            await action(cancellationToken);
        }
    }

    private class FakeLocationSettingsService : ILocationSettingsService
    {
        private readonly IReadOnlyList<string> _locations;

        public FakeLocationSettingsService(IReadOnlyList<string> locations)
        {
            _locations = locations;
        }

        public Task<IReadOnlyList<string>> GetLocationsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_locations);

        public Task UpdateLocationsAsync(
            IReadOnlyList<string> locations,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SeedDefaultLocationsIfEmptyAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}