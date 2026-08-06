using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.ClinicServices.WebApi.ExceptionHandlers;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
     : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "處理 HTTP 請求時發生錯誤。");

        var statusCode = exception switch
        {
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                StatusCodes.Status400BadRequest => "請求處理失敗",
                _ => "伺服器發生錯誤"
            },
            Detail = statusCode switch
            {
                StatusCodes.Status400BadRequest => exception.Message,
                _ => "系統暫時無法處理請求，請稍後再試。"
            },
            
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
