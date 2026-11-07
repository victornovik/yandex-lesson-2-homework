using events.dto;
using events.kafka;

namespace events.endpoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/events");

        // GET /api/events/health
        group.MapGet("/health", () => Results.Ok(new { status = true }));

        // POST /api/events/movie
        group.MapPost("/movie", (MovieEvent e, EventProducer producer) =>
            Publish(producer, KafkaTopics.Movie, e.MovieId, e));

        // POST /api/events/user
        group.MapPost("/user", (UserEvent e, EventProducer producer) =>
            Publish(producer, KafkaTopics.User, e.UserId, e));

        // POST /api/events/payment
        group.MapPost("/payment", (PaymentEvent e, EventProducer producer) =>
            Publish(producer, KafkaTopics.Payment, e.PaymentId, e));
    }

    private static async Task<IResult> Publish<T>(EventProducer producer, string topic, int id, T payload)
    {
        var result = await producer.PublishAsync(topic, id.ToString(), payload);

        return Results.Json(
            data: new { status = "success", result.Topic, Partition = result.Partition.Value, Offset = result.Offset.Value },
            statusCode: StatusCodes.Status201Created);
    }
}
