using proxy.config;
using proxy.endpoints;

var builder = WebApplication.CreateBuilder(args);

// Setup configuration from environment variables
var options = ProxyOptions.FromEnvironment();
builder.Services.AddSingleton(options);

// Setup HTTP Client for forwarding requests to the upstream services
builder.Services.AddHttpClient(ProxyEndpoints.HttpClientName)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false,
        AutomaticDecompression = System.Net.DecompressionMethods.None
    });

// Setup HTTP server to listen on the specified port
builder.WebHost.UseUrls($"http://0.0.0.0:{options.Port}");

var app = builder.Build();
app.MapProxyEndpoints();
app.Run();