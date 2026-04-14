using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Egov.Integrations.MConnect.Events;

/// <summary>
/// Used to configure CloudEvent handlers.
/// </summary>
public class CloudEventHandlersBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of <see cref="CloudEventHandlersBuilder"/>.
    /// </summary>
    /// <param name="services">The services being configured.</param>
    internal CloudEventHandlersBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registers a singleton Cloud Events handler for events with type matching the indicated <paramref name="type"/> with optional data.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <param name="type">Handled event type. Can include wildcard characters ('*' and '?').</param>
    /// <returns>The <see cref="CloudEventHandlersBuilder"/> so that additional calls can be chained.</returns>
    public CloudEventHandlersBuilder AddSingletonHandler<THandler>(string type)
        where THandler : class, IHandleCloudEvents
    {
        _services.TryAddSingleton<THandler>();
        _services.Configure<CloudEventHandlerOptions>(options =>
        {
            options.AddHandlerInvoker(type, new SingletonHandlerInvoker<THandler>());
        });
        return this;
    }

    /// <summary>
    /// Registers a scoped Cloud Events handler for events with type matching the indicated <paramref name="type"/> with optional data.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <param name="type">Handled event type. Can include wildcard characters ('*' and '?').</param>
    /// <returns>The <see cref="CloudEventHandlersBuilder"/> so that additional calls can be chained.</returns>
    public CloudEventHandlersBuilder AddScopedHandler<THandler>(string type)
        where THandler : class, IHandleCloudEvents
    {
        _services.TryAddScoped<THandler>();
        _services.Configure<CloudEventHandlerOptions>(options =>
        {
            options.AddHandlerInvoker(type, new ScopedHandlerInvoker<THandler>());
        });
        return this;
    }

    /// <summary>
    /// Registers a singleton Cloud Events handler for events with type matching the indicated <paramref name="type"/> with required JSON data of particular type.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <typeparam name="TData">The type of the event data.</typeparam>
    /// <param name="type">Handled event type. Can include wildcard characters ('*' and '?').</param>
    /// <param name="serializerOptions">Options to control event data deserialization.</param>
    /// <returns>The <see cref="CloudEventHandlersBuilder"/> so that additional calls can be chained.</returns>
    public CloudEventHandlersBuilder AddSingletonHandler<THandler, TData>(string type, JsonSerializerOptions? serializerOptions = null)
        where THandler : class, IHandleCloudEvents<TData>
    {
        _services.TryAddSingleton<THandler>();
        _services.Configure<CloudEventHandlerOptions>(options =>
        {
            options.AddHandlerInvoker(type, new SingletonDataHandlerInvoker<THandler, TData>(serializerOptions));
        });
        return this;
    }

    /// <summary>
    /// Registers a scoped Cloud Events handler for events with type matching the indicated <paramref name="type"/> with required JSON data of particular type.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <typeparam name="TData">The type of the event data.</typeparam>
    /// <param name="type">Handled event type. Can include wildcard characters ('*' and '?').</param>
    /// <param name="serializerOptions">Options to control event data deserialization.</param>
    /// <returns>The <see cref="CloudEventHandlersBuilder"/> so that additional calls can be chained.</returns>
    public CloudEventHandlersBuilder AddScopedHandler<THandler, TData>(string type, JsonSerializerOptions? serializerOptions = null)
        where THandler : class, IHandleCloudEvents<TData>
    {
        _services.TryAddScoped<THandler>();
        _services.Configure<CloudEventHandlerOptions>(options =>
        {
            options.AddHandlerInvoker(type, new ScopedDataHandlerInvoker<THandler, TData>(serializerOptions));
        });
        return this;
    }

    /// <summary>
    /// Registers a singleton Cloud Events handler for events with type matching the indicated <paramref name="type"/> with optional binary data.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <param name="type">Handled event type. Can include wildcard characters ('*' and '?').</param>
    /// <returns>The <see cref="CloudEventHandlersBuilder"/> so that additional calls can be chained.</returns>
    public CloudEventHandlersBuilder AddSingletonBinaryHandler<THandler>(string type)
        where THandler : class, IHandleCloudEvents<byte[]?>
    {
        _services.TryAddSingleton<THandler>();
        _services.Configure<CloudEventHandlerOptions>(options =>
        {
            options.AddHandlerInvoker(type, new SingletonBinaryHandlerInvoker<THandler>());
        });
        return this;
    }

    /// <summary>
    /// Registers a scoped Cloud Events handler for events with type matching the indicated <paramref name="type"/> with optional binary data.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <param name="type">Handled event type. Can include wildcard characters ('*' and '?').</param>
    /// <returns>The <see cref="CloudEventHandlersBuilder"/> so that additional calls can be chained.</returns>
    public CloudEventHandlersBuilder AddScopedBinaryHandler<THandler>(string type)
        where THandler : class, IHandleCloudEvents<byte[]?>
    {
        _services.TryAddScoped<THandler>();
        _services.Configure<CloudEventHandlerOptions>(options =>
        {
            options.AddHandlerInvoker(type, new ScopedBinaryHandlerInvoker<THandler>());
        });
        return this;
    }
}
