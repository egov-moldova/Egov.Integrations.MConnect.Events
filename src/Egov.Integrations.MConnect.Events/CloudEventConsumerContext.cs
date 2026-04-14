using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Egov.Integrations.MConnect.Events;

/// <summary>
/// A context that enables the consumers and handlers to consume, confirm and mark dead events.
/// </summary>
public sealed class CloudEventConsumerContext : IDisposable
{
    private static readonly byte[] DeadPrefixBytes = "dead:"u8.ToArray();

    private readonly ILogger _logger;
    private readonly ClientWebSocket _socket;

    private ReadOnlyMemory<byte> _eventBytes;
    private CloudEvent? _event;
    private JsonDocument? _eventDocument;
    private bool _disposed;

    internal CloudEventConsumerContext(ILogger logger, ClientWebSocket socket)
    {
        _logger = logger;
        _socket = socket;
    }

    internal bool Parse(ReadOnlyMemory<byte> messageBytes)
    {
        try
        {
            _eventDocument = JsonDocument.Parse(messageBytes);
            _event = _eventDocument.RootElement.ToCloudEvent();
            _eventBytes = messageBytes;
            return true;
        }
        catch (JsonException)
        {
            _logger.LogError("Failed to parse CloudEvent from message: {message}", Convert.ToBase64String(messageBytes.Span));
            return false;
        }
    }

    /// <summary>
    /// The event to be consumed.
    /// </summary>
    public CloudEvent Event => _event ?? throw new InvalidOperationException();

    /// <summary>
    /// Confirm the event as consumed.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    public async Task ConfirmAsync(CancellationToken cancellationToken = default)
    {
        if (_event?.Offset == null)
        {
            throw new InvalidOperationException("Cannot confirm event without offset number.");
        }

        var confirmMessage = Encoding.UTF8.GetBytes($"confirm:{_event.Offset}");
        await _socket.SendAsync(confirmMessage, WebSocketMessageType.Text, true, cancellationToken);
    }

    /// <summary>
    /// Mark the current event as dead.
    /// </summary>
    /// <param name="reason">The reason for marking the event as dead.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    public Task DeadAsync(string reason, CancellationToken cancellationToken = default)
    {
        return DeadAsync(reason, null, _event!.Id, _eventBytes, cancellationToken);
    }

    /// <summary>
    /// Mark the provided <paramref name="event"/> as dead.
    /// </summary>
    /// <param name="reason">The reason for marking the event as dead.</param>
    /// <param name="event">The event to be persisted as dead.</param>
    /// <param name="serializerOptions">Options to control event data serialization.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    public Task DeadAsync(string reason, CloudEvent @event, JsonSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
    {
        var messageBytes = Encoding.UTF8.GetBytes(@event.ToJsonObject(serializerOptions ?? CloudEvent.DefaultJsonSerializerOptions).ToJsonString());
        return DeadAsync(reason, null, @event.Id, messageBytes, cancellationToken);
    }

    internal Task DeadAsync(string reason, Exception exception, CancellationToken cancellationToken = default)
    {
        return DeadAsync(reason, exception, _event!.Id, _eventBytes, cancellationToken);
    }

    internal async Task DeadAsync(string? reason, Exception? exception, string? id, ReadOnlyMemory<byte> eventBytes, CancellationToken cancellationToken = default)
    {
        _logger.LogError(exception, "Marking event {id} as dead: {reason}", id ?? "(none)", reason ?? "exception");

        var messageBytes = new byte[DeadPrefixBytes.Length + eventBytes.Length];
        Array.Copy(DeadPrefixBytes, messageBytes, DeadPrefixBytes.Length);
        eventBytes.CopyTo(messageBytes.AsMemory(DeadPrefixBytes.Length));

        await _socket.SendAsync(messageBytes, WebSocketMessageType.Text, true, cancellationToken);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _eventDocument?.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Standard finalizer.
    /// </summary>
    ~CloudEventConsumerContext()
    {
        Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
