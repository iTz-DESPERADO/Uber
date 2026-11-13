namespace Uber.Application.Abstractions.Locations;

public interface ILocationSearchService
{
    Task<IReadOnlyList<LocationSuggestion>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);
}

public sealed record LocationSuggestion(
    string DisplayName,
    double Latitude,
    double Longitude,
    string? Type);
