using System.Net.Security;
using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Egov.Integrations.MConnect.Events;

internal sealed class CloudEventsConsumerService : BackgroundService
{
    private readonly ILogger<CloudEventsConsumerService> _logger;
    private readonly MConnectEventsConsumerOptions _options;
    private readonly ICloudEventsConsumer _consumer;

    private ClientWebSocket? _socket;

    public CloudEventsConsumerService(
        ILogger<CloudEventsConsumerService> logger,
        IOptions<MConnectEventsConsumerOptions> options, 
        ICloudEventsConsumer consumer)
    {
        _logger = logger;
        _options = options.Value;
        _consumer = consumer;
    }

    public override void Dispose()
    {
        base.Dispose();
        _socket?.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting CloudEvents consumer service");

        var connectUri = _options.GetConnectUri();

        // it is normal for the socket to be periodically closed by server, so reopen if not stopped
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_socket != null)
            {
                _socket.Dispose();
                await Task.Delay(5000, stoppingToken);
            }

            try
            {
                await ConsumeLoopAsync(connectUri, stoppingToken);
            }
            catch (Exception ex)
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "Restarting consumer loop");
                }
            }
        }
    }

    private async Task ConsumeLoopAsync(Uri connectUri, CancellationToken stoppingToken)
    {
        var httpInvoker = new HttpMessageInvoker(new SocketsHttpHandler
        {
            SslOptions = new SslClientAuthenticationOptions
            {
                ClientCertificateContext = SslStreamCertificateContext.Create(_options.SystemCertificate!, _options.SystemCertificateIntermediaries, true)
            }
        });

        _socket = new ClientWebSocket();
        _socket.Options.AddSubProtocol("cloudevents.json");

        using (var connectTimeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken))
        {
            if (_options.ConnectTimeout > TimeSpan.Zero)
            {
                connectTimeoutTokenSource.CancelAfter(_options.ConnectTimeout);
            }

            await _socket.ConnectAsync(connectUri, httpInvoker, connectTimeoutTokenSource.Token);
        }

        var buffer = new byte[_options.ReceiveBufferSize];
        var closeStatus = WebSocketCloseStatus.NormalClosure;
        while (!stoppingToken.IsCancellationRequested)
        {
            WebSocketReceiveResult receiveResult;
            var receiveTask = _socket.ReceiveAsync(buffer, CancellationToken.None);
            try
            {
                receiveResult = await receiveTask.WaitAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            if (receiveResult.CloseStatus.HasValue) break;
            if (!receiveResult.EndOfMessage)
            {
                closeStatus = WebSocketCloseStatus.MessageTooBig;
                break;
            }

            if (receiveResult.Count == 0) continue;

            var eventBytes = new ReadOnlyMemory<byte>(buffer, 0, receiveResult.Count);

            using var context = new CloudEventConsumerContext(_logger, _socket);
            if (!context.Parse(eventBytes))
            {
                await context.DeadAsync("Invalid CloudEvent", null, null, eventBytes, stoppingToken);
            }

            await _consumer.ConsumeAsync(context, stoppingToken);
        }

        if (_socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await _socket.CloseOutputAsync(closeStatus, null, CancellationToken.None);
        }
    }
}