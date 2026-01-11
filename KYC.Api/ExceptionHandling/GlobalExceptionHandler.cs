using KYC.Controllers;
using Microsoft.AspNetCore.Diagnostics;

namespace KYC.ExceptionHandling;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log system-level error
        logger.LogError(exception, "An unhandled system error occurred");

        var errorResponse = new ErrorResponse
        {
#if DEBUG
            Error = exception.Message,
#else
            Error = "An unexpected error occurred while processing the request."
#endif
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }
}