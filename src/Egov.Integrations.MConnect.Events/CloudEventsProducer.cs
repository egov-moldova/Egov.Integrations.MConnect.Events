using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace Egov.Integrations.MConnect.Events;

internal sealed class CloudEventsProducer : ICloudEventsProducer
{
    private static readonly MediaTypeHeaderValue BatchMediaType = new("application/cloudevents-batch+json");

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public CloudEventsProducer(HttpClient httpClient, IOptions<MConnectEventsProducerOptions> options)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = options.Value.JsonSerializerOptions;
    }

    public async Task ProduceAsync(CloudEvent cloudEvent, CancellationToken cancellationToken = default)
    {
        using var content = cloudEvent.ToHttpContent(_jsonSerializerOptions);
        using var response = await _httpClient.PostAsync("produce/raw", content, cancellationToken);
        await CheckHttpResponseAsync(response);
    }

    public async Task ProduceAsync(IReadOnlyList<CloudEvent> cloudEvents, CancellationToken cancellationToken = default)
    {
        var json = new JsonArray();
        foreach (var cloudEvent in cloudEvents)
        {
            json.Add(cloudEvent.ToJsonObject(_jsonSerializerOptions));
        }

        using var content = JsonContent.Create(json, BatchMediaType);
        using var response = await _httpClient.PostAsync("produce/events", content, cancellationToken);
        await CheckHttpResponseAsync(response);
    }

    private static async Task CheckHttpResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        if (response.Content.Headers.ContentType?.MediaType == MediaTypeNames.Text.Plain)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync(), null, response.StatusCode);
        }
        response.EnsureSuccessStatusCode();
    }
}