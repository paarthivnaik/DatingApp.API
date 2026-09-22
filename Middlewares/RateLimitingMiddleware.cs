using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitingOptions _options;
        private static readonly ConcurrentDictionary<string, ClientStatistics> _clients =
            new ConcurrentDictionary<string, ClientStatistics>();

        public RateLimitingMiddleware(RequestDelegate next, IOptions<RateLimitingOptions> options)
        {
            _next = next;
            _options = options?.Value ?? new RateLimitingOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/values", StringComparison.OrdinalIgnoreCase))
            {
                var clientKey = GetClientIdentifier(context);
                var clientStats = _clients.GetOrAdd(clientKey, _ => new ClientStatistics());

                var window = TimeSpan.FromSeconds(_options.WindowInSeconds > 0 ? _options.WindowInSeconds : 60);
                var limit = _options.RequestsPerMinute > 0 ? _options.RequestsPerMinute : 20;

                if (!clientStats.IsRequestAllowed(limit, window))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests; // 429
                    context.Response.ContentType = "application/json";

                    var responsePayload = new
                    {
                        message = $"Rate limit exceeded. Maximum {limit} requests per minute allowed."
                    };

                    var json = JsonSerializer.Serialize(responsePayload);
                    await context.Response.WriteAsync(json);
                    return;
                }
            }

            await _next(context);
        }

        private static string GetClientIdentifier(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) &&
                !string.IsNullOrWhiteSpace(forwardedFor))
            {
                var ips = forwardedFor.ToString().Split(',');
                if (ips.Length > 0 && !string.IsNullOrWhiteSpace(ips[0]))
                {
                    return ips[0].Trim();
                }
            }

            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                return remoteIp;
            }

            return "127.0.0.1";
        }

        private class ClientStatistics
        {
            private readonly object _lock = new object();
            private readonly Queue<DateTime> _timestamps = new Queue<DateTime>();

            public bool IsRequestAllowed(int maxRequests, TimeSpan window)
            {
                lock (_lock)
                {
                    var now = DateTime.UtcNow;
                    var windowStart = now - window;

                    while (_timestamps.Count > 0 && _timestamps.Peek() <= windowStart)
                    {
                        _timestamps.Dequeue();
                    }

                    if (_timestamps.Count >= maxRequests)
                    {
                        return false;
                    }

                    _timestamps.Enqueue(now);
                    return true;
                }
            }
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
