using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Egov.Integrations.MConnect.Events;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Egov.Extensions.Configuration;

namespace Egov.Integrations.MConnect.Events.Tests;

public class ServiceCollectionExtensionsTests
{
    private static X509Certificate2 CreateTestCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("cn=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
        return new X509Certificate2(cert.Export(X509ContentType.Pfx));
    }

    [Fact]
    public void AddCloudEventsProducer_WithConfiguration_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var cert = CreateTestCertificate();
        services.Configure<SystemCertificateOptions>(options => options.Certificate = cert);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BaseAddress"] = "https://test.mconnect.events/",
                ["Timeout"] = "00:00:30"
            })
            .Build();

        // Act
        services.AddCloudEventsProducer(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<MConnectEventsProducerOptions>>().Value;
        Assert.Equal(new Uri("https://test.mconnect.events/"), options.BaseAddress);
        Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.NotNull(options.SystemCertificate);
        
        var producer = provider.GetService<ICloudEventsProducer>();
        Assert.NotNull(producer);
    }

    [Fact]
    public void AddCloudEventsProducer_WithOptionsAction_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var cert = CreateTestCertificate();

        // Act
        services.AddCloudEventsProducer(options =>
        {
            options.BaseAddress = new Uri("https://test-action.mconnect.events/");
            options.SystemCertificate = cert;
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<MConnectEventsProducerOptions>>().Value;
        Assert.Equal(new Uri("https://test-action.mconnect.events/"), options.BaseAddress);
        Assert.NotNull(options.SystemCertificate);
        
        var producer = provider.GetService<ICloudEventsProducer>();
        Assert.NotNull(producer);
    }

    [Fact]
    public void AddCloudEventsConsumer_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var cert = CreateTestCertificate();
        
        // Act
        services.AddCloudEventsConsumer<TestConsumer>(options =>
        {
            options.BaseAddress = new Uri("wss://test.mconnect.events/");
            options.Group = "test-group";
            options.SystemCertificate = cert;
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<MConnectEventsConsumerOptions>>().Value;
        Assert.Equal(new Uri("wss://test.mconnect.events/"), options.BaseAddress);
        Assert.NotNull(options.SystemCertificate);
        
        var consumer = provider.GetService<TestConsumer>();
        Assert.NotNull(consumer);
        
        var hostedService = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>()
            .FirstOrDefault(s => s.GetType().Name.Contains("CloudEventsConsumerService"));
        Assert.NotNull(hostedService);
    }
    

    private class TestConsumer : ICloudEventsConsumer
    {
        public Task ConsumeAsync(CloudEventConsumerContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
