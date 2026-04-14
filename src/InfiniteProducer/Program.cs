using Egov.Integrations.MConnect.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSystemCertificate(builder.Configuration.GetSection("Certificate"));
builder.Services.AddCloudEventsProducer(builder.Configuration.GetSection("CloudEventsProducer"));

var host = builder.Build();
var producer = host.Services.GetRequiredService<ICloudEventsProducer>();

var count = 1;
while (true)
{
    await producer.ProduceAsync(CreateCloudEvent(count));
    Console.WriteLine($"Produced {count} events");
    count++;

    await producer.ProduceAsync(
    [
        CreateCloudEvent(count),
        CreateCloudEvent(count + 1),
        CreateCloudEvent(count + 2)
    ]);
    Console.WriteLine($"Produced events with property: {count}, {count + 1} and {count + 2}");
    count += 3;

    //await Task.Delay(500);
}

static CloudEvent CreateCloudEvent(int propertyValue) =>
    new()
    {
        Source = new Uri("urn:test"),
        Type = "test",
        Id = Guid.NewGuid().ToString(),
        Data = new { property = propertyValue }
    };