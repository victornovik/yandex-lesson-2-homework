using events.config;
using events.endpoints;
using events.kafka;

var builder = WebApplication.CreateBuilder(args);

// Setup configuration from environment variables
var options = KafkaOptions.FromEnvironment();
builder.Services.AddSingleton(options);

// Setup Kafka
builder.Services.AddSingleton<EventProducer>();
builder.Services.AddHostedService<EventConsumer>();

// Setup HTTP server to listen on the specified port
builder.WebHost.UseUrls($"http://0.0.0.0:{options.Port}");

var app = builder.Build();
app.MapEventEndpoints();
app.Run();