using System.Diagnostics;

namespace ATS_Checker_Job_Application_Web_Service.Middleware
{
    public class StopwatchMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<StopwatchMiddleware> logger;

        public StopwatchMiddleware(RequestDelegate next, ILogger<StopwatchMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public  async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            await next(context);
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            logger.LogInformation("Request {Method} {Path} executed in {ElapsedMilliseconds} ms",
                context.Request.Method, context.Request.Path, elapsedMilliseconds);
        }
    }
}
