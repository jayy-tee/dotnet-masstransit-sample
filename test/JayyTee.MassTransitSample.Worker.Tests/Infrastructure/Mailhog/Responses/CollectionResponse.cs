using System.Text.Json.Serialization;

namespace JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog.Responses;

public class CollectionResponse<T> where T : class
{
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("start")] public int Start { get; set; }
    [JsonPropertyName("items")] public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}
