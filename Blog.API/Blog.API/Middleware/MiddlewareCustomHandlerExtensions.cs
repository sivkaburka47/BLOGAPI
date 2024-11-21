namespace Blog.API.Middleware
{
    public static class MiddlewareCustomHandlerExtensions
    {
        public static void UseMiddlewareHandlerException(this IApplicationBuilder app)
        {
            app.UseMiddleware<MiddlewareCustomHandler>();
        }
    }
}