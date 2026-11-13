using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Uber.Ui.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);

        var isBadRequest = exception is BadHttpRequestException;
        var isConcurrencyConflict = exception is DbUpdateConcurrencyException;
        var isUniqueConstraintConflict = exception is DbUpdateException
        {
            InnerException: SqlException { Number: 2601 or 2627 }
        };
        var isDriverReservationConflict = exception is DbUpdateException
        {
            InnerException: SqlException
            {
                Number: 2601 or 2627,
                Message: var sqlMessage
            }
        } && sqlMessage.Contains("UX_Rides_ActiveDriver", StringComparison.Ordinal);
        var isConflict = isConcurrencyConflict || isUniqueConstraintConflict;
        var problem = new ProblemDetails
        {
            Status = isBadRequest
                ? StatusCodes.Status400BadRequest
                : isConflict
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status500InternalServerError,
            Title = isBadRequest
                ? "The request is invalid"
                : isDriverReservationConflict
                    ? "The driver is already reserved"
                    : isConflict
                    ? "The request conflicts with a previous update"
                    : "An unexpected error occurred",
            Detail = isBadRequest
                ? "The submitted request could not be validated."
                : isDriverReservationConflict
                    ? "This driver already has an active ride. Refresh the page and choose another driver."
                    : isConflict
                    ? "Refresh the page and try again. This action may already have been completed."
                    : "The request could not be completed."
        };

        httpContext.Response.StatusCode = problem.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
