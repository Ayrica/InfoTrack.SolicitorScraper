## InfoTrack Solicitor Scraper

InfoTrack Solicitor Scraper is a application that scrapes solicitor contact details from [solicitors.com](https://www.solicitors.com/conveyancing.html) by **location** (by default using "Conveyancing" area of law.

## Getting Started

1. Review and adjust your locations list on `Locations` page.
2. On the `Locations` page, click **Save** to persist changes or **Scrape** (wait several minutes for scraping to finish) to fetch solicitor data (by default `Solicitors` and `Report` pages are empty).
3. Browse scraped solicitors contact details using filter by location or search by name and view the summary report.

## Solution structure

| Project | Responsibility |
|---------|----------------|
| `InfoTrack.SolicitorScraper.Api` | HTTP endpoints, CORS, Swagger |
| `InfoTrack.SolicitorScraper.Application` | Use cases, DTOs, orchestration |
| `InfoTrack.SolicitorScraper.Core` | Domain entities |
| `InfoTrack.SolicitorScraper.Infrastructure` | Custom HTML scraping, EF Core InMemory persistence |
| `InfoTrack.SolicitorScraper.Tests` | Unit tests |

## Technologies used

| Area | Technology |
|------|------------|
| **Language & runtime** | C#, .NET 8 |
| **Backend API** | ASP.NET Core Web API |
| **Frontend** | Blazor WebAssembly (SPA) |
| **Database** | Entity Framework Core 8 - InMemory provider |
| **API documentation** | Swagger / OpenAPI (Swashbuckle.AspNetCore) |
| **HTTP & scraping** | `HttpClient`, custom regex/string parsing in `SolicitorsHtmlParser`  |
| **Architecture** | Layered solution - Api, Application, Core, Infrastructure, Web |
| **Testing** | xUnit, `Microsoft.AspNetCore.Mvc.Testing` |
| **Configuration** | `appsettings.json`, CORS, HTTPS |

## API endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/locations` | Current location list |
| `PUT` | `/api/locations` | Update locations |
| `POST` | `/api/scrape` | Scrape configured or specified locations |
| `GET` | `/api/solicitors?locations=London,Manchester` | Query stored contacts |
| `GET` | `/api/reports/summary` | Report for SPA |

Full list of endpoints you can find in `InfoTrack.SolicitorScraper.Api.http` file.

## Run the API

```powershell
cd InfoTrack.SolicitorScraper
dotnet run --project InfoTrack.SolicitorScraper.Api
```
Swagger URL: `https://localhost:5001/swagger`

## Run the WEB UI

```powershell
cd InfoTrack.SolicitorScraper
dotnet run --project InfoTrack.SolicitorScraper.Web
```
URL: `http://localhost:5029`

## Run tests

```powershell
cd InfoTrack.SolicitorScraper
dotnet test
```