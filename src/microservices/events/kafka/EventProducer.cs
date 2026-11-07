using System.Text.Json;
using Confluent.Kafka;
using events.config;

namespace events.kafka;

public class EventProducer(KafkaOptions options, ILogger<EventProducer> logger)
{
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(
        new ProducerConfig
        {
            BootstrapServers = options.Brokers
        }).Build();

    public async Task<DeliveryResult<string, string>> PublishAsync<T>(string topic, string key, T payload)
    {
        var result = await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = JsonSerializer.Serialize(payload)
        });

        logger.LogInformation("Produced to {Topic}: {Value}", topic, result.Value);
        return result;
    }
}