using Confluent.Kafka;
using events.config;

namespace events.kafka;

// Reads events from all topics and writes them to the log
public class EventConsumer(KafkaOptions options, ILogger<EventConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken ct) => Task.Run(() =>
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = options.Brokers,
            GroupId = "events-service",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(KafkaTopics.All);

        while (true)
        {
            try
            {
                var result = consumer.Consume();
                logger.LogInformation("Consumed from {Topic}: {Value}", result.Topic, result.Message.Value);
            }
            catch (ConsumeException ex)
            {
                logger.LogWarning("Consume error: {Reason}", ex.Error.Reason);
            }
        }
    }, ct);
}
