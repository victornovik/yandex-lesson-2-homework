using proxy.config;

namespace proxy.endpoints;

public static class ProxyEndpoints
{
    public const string HttpClientName = "upstream";

    // Headers that must not be copied between connections
    private static readonly HashSet<string> SkippedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Host", "Connection", "Keep-Alive", "Transfer-Encoding", "Upgrade"
    };
    
    public static void MapProxyEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = true }));

        // Map all HTTP methods and requests to the Forward method, which will handle routing to the appropriate upstream service
        app.Map("/{**path}", Forward);
    }

    private static async Task Forward(HttpContext context, ProxyOptions options, IHttpClientFactory factory, ILogger<Program> logger)
    {
        var request = context.Request;
        var baseUrl = SelectUpstream(request.Path, options);
        var destUrl = new Uri(baseUrl + request.Path + request.QueryString);

        using var upstreamRequest = new HttpRequestMessage(new HttpMethod(request.Method), destUrl);

        // Forward the request body, if any
        if (request.ContentLength > 0 || request.Headers.ContainsKey("Transfer-Encoding"))
        {
            upstreamRequest.Content = new StreamContent(request.Body);
        }

        // Copy headers from the incoming request to the upstream request.
        // Content-* headers belong to the content, the rest to the request itself.
        foreach (var header in request.Headers)
        {
            if (SkippedHeaders.Contains(header.Key)) 
                continue;

            if (!upstreamRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                upstreamRequest.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        logger.LogInformation("Forward {Method} {Path} -> {Target}", request.Method, request.Path, baseUrl);

        try
        {
            var httpClient = factory.CreateClient(HttpClientName);

            using var upstreamResponse = await httpClient.SendAsync(upstreamRequest);

            context.Response.StatusCode = (int)upstreamResponse.StatusCode;

            // Copy headers from the upstream response
            foreach (var header in upstreamResponse.Headers.Concat(upstreamResponse.Content.Headers))
            {
                if (SkippedHeaders.Contains(header.Key)) 
                    continue;
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            await upstreamResponse.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Upstream {Target} is unavailable", baseUrl);
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
        }
    }

    // Strangler Fig: requests are routed between monolith and microservice
    private static string SelectUpstream(PathString path, ProxyOptions options)
    {
        if (path.StartsWithSegments("/api/events"))
            return options.EventsServiceUrl;

        if (path.StartsWithSegments("/api/movies") && options.RouteToMoviesMicroservice())
            return options.MoviesServiceUrl;

        return options.MonolithUrl;
    }
}