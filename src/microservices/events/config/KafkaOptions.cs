namespace events.config;

public record KafkaOptions(int Port, string Brokers, string GroupId)
{
    public static KafkaOptions FromEnvironment() => new(
        Port: ReadInt("PORT", 8082),
        Brokers: Read("KAFKA_BROKERS", "kafka:9092"),
        GroupId: Read("KAFKA_GROUP_ID", "events-service"));

    private static string Read(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
    }

    private static int ReadInt(string name, int defaultValue)
    {
        return int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;
    }
}