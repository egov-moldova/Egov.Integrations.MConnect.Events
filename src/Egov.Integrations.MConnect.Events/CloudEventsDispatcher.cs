using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Egov.Integrations.MConnect.Events;

internal sealed class CloudEventsDispatcher : ICloudEventsConsumer
{
    private readonly ILogger<CloudEventsDispatcher> _logger;
    private readonly CloudEventHandlerOptions _options;
    private readonly IServiceProvider _provider;

    public CloudEventsDispatcher(
        ILogger<CloudEventsDispatcher> logger, 
        IOptions<CloudEventHandlerOptions> options, 
        IServiceProvider provider)
    {
        _logger = logger;
        _options = options.Value;
        _provider = provider;
    }

    public Task ConsumeAsync(CloudEventConsumerContext context, CancellationToken cancellationToken)
    {
        if (!_options.TryGetHandlerInvoker(context.Event.Type, out var handlerInvoker))
        {
            _logger.LogError($"Ignoring event as no handler registered for cloud event type: {context.Event.Type}");
            return Task.CompletedTask;
        }

        return handlerInvoker.InvokeHandler(_provider, context, cancellationToken);
    }
}