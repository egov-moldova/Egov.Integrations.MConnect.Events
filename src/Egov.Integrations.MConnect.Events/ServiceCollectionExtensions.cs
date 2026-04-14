using System.Net.Security;
using Egov.Extensions.Configuration;
using Egov.Integrations.MConnect.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for CloudEvent producers and consumers.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddCloudEventsProducer(this IServiceCollection services)
    {
        services.AddOptions<MConnectEventsProducerOptions>()
            .Configure<IOptions<SystemCertificateOptions>>((options, systemCertificateOptions) =>
            {
                options.SystemCertificate ??= systemCertificateOptions.Value.Certificate;
            });

        services.AddHttpClient<ICloudEventsProducer, CloudEventsProducer>()
            .ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<MConnectEventsProducerOptions>>().Value;
                client.BaseAddress = options.BaseAddress;
                client.Timeout = options.Timeout;
            })
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                var options = provider.GetRequiredService<IOptions<MConnectEventsProducerOptions>>().Value;
                return new SocketsHttpHandler
                {
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        ClientCertificateContext = SslStreamCertificateContext.Create(options.SystemCertificate!, options.SystemCertificateIntermediaries, true)
                    }
                };
            });
        return services;
    }

    private static IServiceCollection AddCloudEventsConsumer<TConsumer>(this IServiceCollection services)
        where TConsumer : class, ICloudEventsConsumer
    {
        services.AddOptions<MConnectEventsConsumerOptions>()
            .Configure<IOptions<SystemCertificateOptions>>((options, systemCertificateOptions) =>
            {
                var systemCertificateOptionsValue = systemCertificateOptions.Value;
                options.SystemCertificate ??= systemCertificateOptionsValue.Certificate;
                options.SystemCertificateIntermediaries = systemCertificateOptionsValue.IntermediateCertificates;
            });

        services.AddSingleton<TConsumer>();
        services.AddHostedService(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CloudEventsConsumerService>>();
            var options = provider.GetRequiredService<IOptions<MConnectEventsConsumerOptions>>();
            var consumer = provider.GetRequiredService<TConsumer>();
            return new CloudEventsConsumerService(logger, options, consumer);
        });
        return services;
    }

    /// <summary>
    /// Register services required to produce events.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">The configuration being bound.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCloudEventsProducer(this IServiceCollection services, 
        IConfiguration config)
    {
        services.Configure<MConnectEventsProducerOptions>(config);
        return services.AddCloudEventsProducer();
    }

    /// <summary>
    /// Register services required to produce events.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCloudEventsProducer(this IServiceCollection services,
        Action<MConnectEventsProducerOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services.AddCloudEventsProducer();
    }

    /// <summary>
    /// Register services required to produce events.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">The configuration being bound.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCloudEventsProducer(this IServiceCollection services,
        IConfiguration config,
        Action<MConnectEventsProducerOptions> configureOptions)
    {
        services.Configure<MConnectEventsProducerOptions>(config);
        return services.AddCloudEventsProducer(configureOptions);
    }

    /// <summary>
    /// Registers a particular type of CloudEvent consumer.
    /// </summary>
    /// <typeparam name="TConsumer">The type of the consumer to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">The configuration being bound.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCloudEventsConsumer<TConsumer>(this IServiceCollection services,
        IConfiguration config)
        where TConsumer : class, ICloudEventsConsumer
    {
        services.Configure<MConnectEventsConsumerOptions>(config);
        return services.AddCloudEventsConsumer<TConsumer>();
    }

    /// <summary>
    /// Registers a particular type of CloudEvent consumer.
    /// </summary>
    /// <typeparam name="TConsumer">The type of the consumer to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCloudEventsConsumer<TConsumer>(this IServiceCollection services, 
        Action<MConnectEventsConsumerOptions> configureOptions)
        where TConsumer : class, ICloudEventsConsumer
    {
        services.Configure(configureOptions);
        return services.AddCloudEventsConsumer<TConsumer>();
    }

    /// <summary>
    /// Registers a particular type of CloudEvent consumer.
    /// </summary>
    /// <typeparam name="TConsumer">The type of the consumer to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">The configuration being bound.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCloudEventsConsumer<TConsumer>(this IServiceCollection services,
        IConfiguration config,
        Action<MConnectEventsConsumerOptions> configureOptions)
        where TConsumer : class, ICloudEventsConsumer
    {
        services.Configure<MConnectEventsConsumerOptions>(config);
        return services.AddCloudEventsConsumer<TConsumer>(configureOptions);
    }

    /// <summary>
    /// Registers services required by CloudEvent handlers.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">The configuration being bound.</param>
    /// <returns>A <see cref="CloudEventHandlersBuilder"/> that can be used to further configure handlers.</returns>
    public static CloudEventHandlersBuilder AddCloudEventHandlers(this IServiceCollection services, 
        IConfiguration config)
    {
        services.AddCloudEventsConsumer<CloudEventsDispatcher>(config);
        return new CloudEventHandlersBuilder(services);
    }

    /// <summary>
    /// Registers services required by CloudEvent handlers.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>A <see cref="CloudEventHandlersBuilder"/> that can be used to further configure handlers.</returns>
    public static CloudEventHandlersBuilder AddCloudEventHandlers(this IServiceCollection services,
        Action<MConnectEventsConsumerOptions> configureOptions)
    {
        services.AddCloudEventsConsumer<CloudEventsDispatcher>(configureOptions);
        return new CloudEventHandlersBuilder(services);
    }

    /// <summary>
    /// Registers services required by CloudEvent handlers.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">The configuration being bound.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    /// <returns>A <see cref="CloudEventHandlersBuilder"/> that can be used to further configure handlers.</returns>
    public static CloudEventHandlersBuilder AddCloudEventHandlers(this IServiceCollection services,
        IConfiguration config,
        Action<MConnectEventsConsumerOptions> configureOptions)
    {
        services.AddCloudEventsConsumer<CloudEventsDispatcher>(config);
        return services.AddCloudEventHandlers(configureOptions);
    }
}