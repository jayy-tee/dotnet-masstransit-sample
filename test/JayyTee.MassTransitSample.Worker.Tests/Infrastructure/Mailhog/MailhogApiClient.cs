using System.Text.Json;
using JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog.Responses;
using MassTransit.Futures.Contracts;

namespace JayyTee.MassTransitSample.Worker.Tests.Infrastructure.Mailhog;

public record MailhogMessage
{
    public ICollection<string> Recipients { get; set; }
    public string FromAddress { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;

}


public class MailhogApiClient : IDisposable
{
    private readonly MailhogSettings _settings;
    private readonly HttpClient _httpClient;

    public MailhogApiClient(MailhogSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_settings.ApiBaseUri);
    }

    public async Task DeleteAllMessagesAsync()
    {
        await _httpClient.DeleteAsync("/api/v1/messages");
    }

    public async Task<IEnumerable<MailhogMessage>> FindMessagesByRecipient(string recipientAddress)
    {
        using var httpResponse = await _httpClient.GetAsync($"/api/v2/search?kind=to&query={recipientAddress}");

        var responseText = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<CollectionResponse<Message>>(responseText);

        var results = new List<MailhogMessage>();

        if (response is null) return results;

        foreach (var item in response.Items)
        {
            results.Add(new MailhogMessage
            {
                Recipients = item.To
                    .Select(to => $"{to.Mailbox}@{to.Domain}")
                    .ToArray()
                ,
                FromAddress = $"{item.From.Mailbox}@{item.From.Domain}",
                Subject = item.Content.Headers.Subject?.FirstOrDefault() ?? string.Empty,
                Body = item.Content.Body
            });
        }


        return results;
    }

    public void Dispose() => _httpClient.Dispose();
}
