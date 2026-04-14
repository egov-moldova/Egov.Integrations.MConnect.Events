namespace Egov.Integrations.MConnect.Events;

/// <summary>
/// An interface that represents a CloudEvent consumer.
/// </summary>
public interface ICloudEventsConsumer
{
    /// <summary>
    /// Consumes a CloudEvent.
    /// The consumer shall confirm the event as consumed or dead via the provided <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The <see cref="CloudEventConsumerContext"/> that includes event to handle.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for during event consumption.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task ConsumeAsync(CloudEventConsumerContext context, CancellationToken cancellationToken = default);
}