using Serilog;
using System.Net;
using System.Text.Json;

namespace OOB.Logger
{
    public class ErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly Serilog.ILogger _logger = Log.ForContext<ErrorHandler>();

        public ErrorHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                _logger.Error(ex, "An unhandled exception occurred while processing the request.");

                switch (ex)
                {
                    case KeyNotFoundException e:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = JsonSerializer.Serialize(new { message = ex?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}
