using FluentValidation;

namespace ATS_Checker_Job_Application_Web_Service.Middleware
{
    public sealed class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //. adding new middelware and this will (catch) the validation exception and return a 400 bad request with the validation errors
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException exception)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    IsSuccess = false,
                    Message = "Request validation failed.",
                    Errors = exception.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(group => group.Key, 
                                      group => group.Select(error => error.ErrorMessage).ToArray())
                });
            }
        }
    }
}
