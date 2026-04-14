namespace Egov.Integrations.MConnect.Events;

/// <summary>
/// An interface that represents a CloudEvent producer.
/// </summary>
public interface ICloudEventsProducer
{
    /// <summary>
    /// Produce a CloudEvent.
    /// </summary>
    /// <param name="cloudEvent">The event to produce.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for during event producing.</param>
    /// <returns>A task that represents the asynchronous produce operation.</returns>
    Task ProduceAsync(CloudEvent cloudEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Produce a batch of CloudEvents.
    /// </summary>
    /// <param name="cloudEvents">The events to produce.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for during event producing.</param>
    /// <returns>A task that represents the asynchronous produce operation.</returns>
    Task ProduceAsync(IReadOnlyList<CloudEvent> cloudEvents, CancellationToken cancellationToken = default);
}