using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrackingSystem.WebAPI.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An Unexpected Error Occured: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // RFC 7807 standard Format.
        context.Response.ContentType = "application/problem+json";

        var statusCode = exception switch
        {
            KeyNotFoundException => HttpStatusCode.NotFound,  
            InvalidOperationException => HttpStatusCode.BadRequest,  
            UnauthorizedAccessException => HttpStatusCode.Unauthorized, 
            _ => HttpStatusCode.InternalServerError 
        };

        context.Response.StatusCode = (int)statusCode;


        var problemDetails = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Type = $"https://httpstatuses.io/{(int)statusCode}", 
            Title = GetTitleForStatus(statusCode),
            Detail = exception.Message,
            Instance = context.Request.Path 
        };

        if (_env.IsDevelopment())
        {
            problemDetails.Extensions.Add("stackTrace", exception.StackTrace);
        }

        var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        return context.Response.WriteAsync(jsonResponse);
    }

    private static string GetTitleForStatus(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => "Resource Not Found",
            HttpStatusCode.BadRequest => "Bad Request Business Error",
            HttpStatusCode.Unauthorized => "Unauthorized Access",
            _ => "An Unexpected Internal Server Error Occurred"
        };
    }
}