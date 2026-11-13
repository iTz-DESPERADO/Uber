using Uber.Domain.Common;

namespace Uber.Ui.Endpoints.Common;

public static class ResultEndpointExtensions
{
    public static IResult ToProblem(this Result result)
    {
        return Results.Problem(
            title: result.Error?.Code,
            detail: result.Error?.Message,
            statusCode: StatusCodes.Status400BadRequest);
    }
}
