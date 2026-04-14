using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Egov.Integrations.MConnect.Events;

internal abstract class HandlerInvoker
{
    public abstract Task InvokeHandler(IServiceProvider provider, CloudEventConsumerContext context, CancellationToken cancellationToken);
}

internal sealed class SingletonHandlerInvoker<THandler> : HandlerInvoker
    where THandler : class, IHandleCloudEvents
{
    private THandler? _handler;

    public override Task InvokeHandler(IServiceProvider provider, CloudEventConsumerContext context, CancellationToken cancellationToken)
    {
        _handler ??= provider.GetRequiredService<THandler>();
        return _handler.HandleAsync(context, cancellationToken);
    }
}

internal sealed class ScopedHandlerInvoker<THandler> : HandlerInvoker
    where THandler : class, IHandleCloudEvents
{
    public override Task InvokeHandler(IServiceProvider provider, CloudEventConsumerContext context, CancellationToken cancellationToken)
    {
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<THandler>();
        return handler.HandleAsync(context, cancellationToken);
    }
}

internal sealed class SingletonBinaryHandlerInvoker<THandler> : HandlerInvoker
    where THandler : class, IHandleCloudEvents<byte[]?>
{
    private THandler? _handler;

    public override Task InvokeHandler(IServiceProvider provider, CloudEventConsumerContext context, CancellationToken cancellationToken)
    {
        _handler ??= provider.GetRequiredService<THandler>();
        return _handler.HandleAsync(context, context.Event.DeserializeBinaryData(), cancellationToken);
    }
}

internal sealed class ScopedBinaryHandlerInvoker<THandler> : HandlerInvoker
    where THandler : class, IHandleCloudEvents<byte[]?>
{
    public override Task InvokeHandler(IServiceProvider provider, CloudEventConsumerContext context, CancellationToken cancellationToken)
    {
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<THandler>();
        return handler.HandleAsync(context, context.Event.DeserializeBinaryData(), cancellationToken);
    }
}

internal sealed class SingletonDataHandlerInvoker<THandler, TData> : HandlerInvoker
    where THandler: class, IHandleCloudEvents<TData>
{
    private readonly JsonSerializerOptions? _jsonSerializerOptions;
    private THandler? _handler;

    public SingletonDataHandlerInvoker(JsonSerializerOptions? jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public override async Task InvokeHandler(IServiceProvider provider, CloudEventConsumerContext context, CancellationToken cancellationToken)
    {
        TData? data;
        try
        {
            data = context.Event.DeserializeData<TData>(_jsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            await context.DeadAsync($"Failed to deserialize data for event type: {context.Event.Type}", ex, cancellationToken);
            return;
        }

        if (data == null)
        {
            await context.DeadAsync($"Empty data for event type: {context.Event.Type}", cancellationToken);
            return;
        }

        _handler ??= provider.GetRequiredService<THandler>();
        await _handler.HandleAsync(context, data, cancellationToken);
    }
}

internal sealed class ScopedDataHandlerInvoker<THandler, TData> : HandlerInvoker
    where THandler : class, IHandleCloudEvents<TData>
{
    private readonly JsonSerializerOptions? _jsonSerializerOptions;

    public ScopedDataHandlerInvoker(JsonSerializerOptions? jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public override async Task InvokeHandler(IServiceProvider provider, CloudEventConsumerContext context, CancellationToken cancellationToken)
    {
        TData? data;
        try
        {
            data = context.Event.DeserializeData<TData>(_jsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            await context.DeadAsync($"Failed to deserialize data for event type: {context.Event.Type}", ex, cancellationToken);
            return;
        }

        if (data == null)
        {
            await context.DeadAsync($"Empty data for event type: {context.Event.Type}", cancellationToken);
            return;
        }

        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<THandler>();
        await handler.HandleAsync(context, data, cancellationToken);
    }
}