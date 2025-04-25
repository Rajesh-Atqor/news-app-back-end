using System.Net;

namespace HackerNews.Middlewares
{
    public class ErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env; // To differentiate behavior for development/production environments

        public ErrorHandler(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                // Call the next middleware in the pipeline
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Set the content type to JSON
            context.Response.ContentType = "application/json";

            // Determine the appropriate status code and error message based on the exception type
            var statusCode = StatusCodes.Status500InternalServerError; // Default to 500 (Internal Server Error)
            string message = "An unexpected error occurred"; // Default message for generic errors

            // Custom behavior for different types of exceptions
            switch (exception)
            {
                case BadHttpRequestException:
                    statusCode = StatusCodes.Status400BadRequest; // Bad Request
                    message = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized; // Unauthorized
                    message = "Unauthorized access";
                    break;

                case KeyNotFoundException:
                    statusCode = StatusCodes.Status404NotFound; // Not Found
                    message = "Resource not found";
                    break;

                case HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Accepted:
                    statusCode = StatusCodes.Status202Accepted;
                    message = httpEx.Message;
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError; // Internal Server Error
                    if (_env.IsDevelopment())
                    {
                        message = exception.Message; // Detailed message in development
                    }
                    break;
            }
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = statusCode,
                Message = message,
                TraceId = context.TraceIdentifier
            });
        }
    }
}
