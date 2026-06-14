using System.Diagnostics;

namespace WebApi.MiddleWare;

public class LoggingMiddleWare
{
    private readonly RequestDelegate _next;
    public LoggingMiddleWare(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        System.Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
        System.Console.WriteLine($"Request body: {context.Request.BodyReader}");
        stopwatch.Start();

        await _next(context);

        stopwatch.Stop();
        System.Console.WriteLine($"Response: {context.Response.StatusCode} processed in {stopwatch.ElapsedMilliseconds} ms");
    }
}