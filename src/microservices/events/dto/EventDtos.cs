using System.Text.Json.Serialization;

namespace events.dto;

public record MovieEvent(
    [property: JsonPropertyName("movie_id")] int MovieId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("user_id")] int? UserId);

public record UserEvent(
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("username")] string? Username,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("timestamp")] DateTime? Timestamp);

public record PaymentEvent(
    [property: JsonPropertyName("payment_id")] int PaymentId,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("timestamp")] DateTime? Timestamp,
    [property: JsonPropertyName("method_type")] string? MethodType);
