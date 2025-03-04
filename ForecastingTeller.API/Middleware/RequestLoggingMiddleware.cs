using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Log the request details
            LogRequest(context);

            // Process the request
            await _next(context);

            // Log the response details
            stopwatch.Stop();
            LogResponse(context, stopwatch.ElapsedMilliseconds);
        }

        private void LogRequest(HttpContext context)
        {
            var request = context.Request;
            
            _logger.LogInformation(
                "Request: {Method} {Path}{QueryString} | " +
                "Client: {ClientIp} | " +
                "User-Agent: {UserAgent}",
                request.Method,
                request.Path,
                request.QueryString,
                context.Connection.RemoteIpAddress,
                request.Headers["User-Agent"].ToString());
        }

        private void LogResponse(HttpContext context, long elapsedMs)
        {
            var response = context.Response;
            
            _logger.LogInformation(
                "Response: {StatusCode} | " +
                "Took: {ElapsedMilliseconds}ms",
                response.StatusCode,
                elapsedMs);
        }
    }
}