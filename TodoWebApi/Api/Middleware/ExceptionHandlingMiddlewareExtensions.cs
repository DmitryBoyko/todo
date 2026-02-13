namespace TodoWebApi.Api.Middleware
{
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
