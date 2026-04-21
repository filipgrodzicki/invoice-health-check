using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceHealthCheck.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in request to {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation in request to {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status409Conflict,
                title: "Operation cannot be completed",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in request to {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                title: "An unexpected error occurred",
                detail: "Please try again later.");
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}