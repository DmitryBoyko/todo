using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace TodoWebApi.Api.Middleware
{

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IProblemDetailsService problemDetailsService,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _problemDetailsService = problemDetailsService;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // Корректно обрабатываем отмену запроса, без 500
                _logger.LogWarning("Request was cancelled. TraceId: {TraceId}", context.TraceIdentifier);
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest; // Nginx style, но можно 400
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/500",
                    Title = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = context.Request.Path
                };

                if (_env.IsDevelopment())
                {
                    problemDetails.Extensions["detail"] = ex.Message;
                    problemDetails.Extensions["stackTrace"] = ex.StackTrace;
                }

                await _problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = problemDetails
                });
            }
        }
    }
}
