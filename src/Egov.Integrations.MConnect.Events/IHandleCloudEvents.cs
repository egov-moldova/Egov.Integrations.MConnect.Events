namespace Egov.Integrations.MConnect.Events;

/// <summary>
/// An interface that defines a generic CloudEvent handler.
/// </summary>
public interface IHandleCloudEvents
{
    /// <summary>
    /// Handles a CloudEvent.
    /// The handler shall confirm the event as consumed or dead via the provided <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The <see cref="CloudEventConsumerContext"/> that includes event to handle.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for during event handling.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task HandleAsync(CloudEventConsumerContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// An interface that defines a CloudEvent handler for events containing strongly-type data as indicated by <typeparamref name="TData"/>.
/// </summary>
/// <typeparam name="TData">The type of the event data.</typeparam>
public interface IHandleCloudEvents<in TData>
{
    /// <summary>
    /// Handles a CloudEvent containing data of <typeparamref name="TData"/> type.
    /// The handler shall confirm the event as consumed or dead via the provided <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The <see cref="CloudEventConsumerContext"/> that includes event to handle.</param>
    /// <param name="data">Strongly typed event data.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for during event handling.</param>
    /// <returns>A task that represents the asynchronous handling operation.</returns>
    Task HandleAsync(CloudEventConsumerContext context, TData data, CancellationToken cancellationToken = default);
}