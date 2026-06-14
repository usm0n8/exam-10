using System.Text.Json;
using Npgsql;

namespace WebApi.MiddleWare;

public class ExceptionMiddleware
{


    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    public ExceptionMiddleware(RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex.Message);

            context.Response.StatusCode = 400;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    new
                    {
                        Error = ex.Message
                    }
                )
            );
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error");

            context.Response.StatusCode = 400;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    new
                    {
                        Error = "Database error"
                    }
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = 500;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    new
                    {
                        Error = "Internal server error"
                    }
                )
            );
        }


    }
}