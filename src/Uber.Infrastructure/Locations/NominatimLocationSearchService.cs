using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Uber.Application.Abstractions.Locations;

namespace Uber.Infrastructure.Locations;

internal sealed class NominatimLocationSearchService(HttpClient client)
    : ILocationSearchService
{
    public async Task<IReadOnlyList<LocationSuggestion>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            return [];
        }

        var url = $"search?format=jsonv2&addressdetails=1&limit=6&countrycodes=eg&accept-language=ar,en&q={Uri.EscapeDataString(query.Trim())}";
        var results = await client.GetFromJsonAsync<NominatimResult[]>(url, cancellationToken) ?? [];

        return results
            .Select(result => new LocationSuggestion(
                result.DisplayName,
                double.Parse(result.Latitude, CultureInfo.InvariantCulture),
                double.Parse(result.Longitude, CultureInfo.InvariantCulture),
                result.Type))
            .ToArray();
    }

    private sealed record NominatimResult(
        [property: JsonPropertyName("display_name")] string DisplayName,
        [property: JsonPropertyName("lat")] string Latitude,
        [property: JsonPropertyName("lon")] string Longitude,
        [property: JsonPropertyName("type")] string? Type);
}
