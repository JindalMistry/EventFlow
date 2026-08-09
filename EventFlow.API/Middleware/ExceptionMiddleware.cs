using EventFlow.Application.Common;
using EventFlow.Application.Exceptions;
using System.Text.Json;

namespace EventFlow.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
        {
            var response = new ErrorResponse();

            switch (exception)
            {
                case BadRequestException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case UnauthorizedException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    break;

                case ForbiddenException:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    break;

                case NotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    break;

                default:
                    context.Response.StatusCode =
                        StatusCodes.Status500InternalServerError;
                    break;
            }

            response.Message = exception.Message;

            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}