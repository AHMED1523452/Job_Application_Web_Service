using FluentValidation;

namespace ATS_Checker_Job_Application_Web_Service.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        //. specific exception handling for FluentValidation.ValidationException
        catch (ValidationException exception)
        {
            _logger.LogWarning(exception, "Request validation failed for {Method} {Path}.",
                context.Request.Method, context.Request.Path);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                IsSuccess = false,
                Message = "Request validation failed.",
                Errors = exception.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())
            }, context.RequestAborted);
        }

        //. General exception handling for unhandled exceptions
        //. any exception will be catched 
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}.",
                context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                IsSuccess = false,
                Message = exception.Message,
                TraceId = context.TraceIdentifier
            }, context.RequestAborted);
        }
    }
}
