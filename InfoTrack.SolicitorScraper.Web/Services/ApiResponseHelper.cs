using System.Net.Http.Json;
using InfoTrack.SolicitorScraper.Web.Models;

namespace InfoTrack.SolicitorScraper.Web.Services;

internal static class ApiResponseHelper
{
    public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken);
            if (!string.IsNullOrWhiteSpace(error?.Error))
            {
                throw new ApiException(error.Error);
            }
        }
        catch (ApiException)
        {
            throw;
        }
        catch
        {
            // Fall back to generic message below.
        }

        throw new ApiException($"Request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
    }
}