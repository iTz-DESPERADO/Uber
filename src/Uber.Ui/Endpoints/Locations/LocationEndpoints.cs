using Uber.Application.Abstractions.Locations;

namespace Uber.Ui.Endpoints.Locations;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/locations/search", SearchAsync)
            .WithTags("Locations");

        return endpoints;
    }

    private static async Task<IResult> SearchAsync(
        string query,
        ILocationSearchService locations,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            return Results.Ok(Array.Empty<LocationSuggestion>());
        }

        var results = await locations.SearchAsync(query, cancellationToken);
        return Results.Ok(results);
    }
}
