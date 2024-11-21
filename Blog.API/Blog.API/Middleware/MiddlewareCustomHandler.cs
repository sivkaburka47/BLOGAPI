using System.Net;
using System.Text.Json;
using Blog.API.Models.DTOs;
using Blog.API.Services;

namespace Blog.API.Middleware
{
    public class MiddlewareCustomHandler
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public MiddlewareCustomHandler(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var tokenBlackListService = scope.ServiceProvider.GetRequiredService<ITokenBlackListService>();

                var token = context.Request.Headers["Authorization"].ToString()?.Split(" ").Last();
                if (!string.IsNullOrEmpty(token) && await tokenBlackListService.iSTokenRevoked(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token is revoked");
                    return;
                }
            }

            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            object response;

            switch (exception)
            {
                case ValidationAccessException:
                    status = HttpStatusCode.BadRequest;
                    response = new { status = "Invalid Arguments", message = exception.Message };
                    break;

                case UnauthorizedAccessException:
                    status = HttpStatusCode.Unauthorized;
                    response = new { status = "Unauthorized", message = exception.Message };
                    break;

                case ForbiddenAccessException:
                    status = HttpStatusCode.Forbidden;
                    response = new { status = "Access Is Forbidden", message = exception.Message };
                    break;

                case KeyNotFoundException:
                    status = HttpStatusCode.NotFound;
                    response = new { status = "Not Found", message = exception.Message };
                    break;

                default:
                    status = HttpStatusCode.InternalServerError;
                    response = new Response { status = "InternalServiceError", message = "An unexpected error occurred" };
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;
            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}