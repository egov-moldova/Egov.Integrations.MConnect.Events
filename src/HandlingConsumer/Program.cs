using System.Text;
using Egov.Integrations.MConnect.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSystemCertificate(builder.Configuration.GetSection("Certificate"));
builder.Services.AddCloudEventHandlers(builder.Configuration.GetSection("CloudEventsConsumer"))
    .AddSingletonHandler<TestEventHandler, TestEventData>("*");

var app = builder.Build();
app.Run();

class TestEventData
{
    public int Property { get; set; }
}

class TestEventHandler : IHandleCloudEvents<TestEventData>
{
    public async Task HandleAsync(CloudEventConsumerContext context, TestEventData data, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Consumed event of type {context.Event.Type} with property = {data.Property} and offset = {context.Event.Offset}");
        await context.ConfirmAsync(cancellationToken);
    }
}

class BinaryEventHandler : IHandleCloudEvents<byte[]?>
{
    public async Task HandleAsync(CloudEventConsumerContext context, byte[]? data, CancellationToken cancellationToken)
    {
        var dataString = data != null ? Encoding.UTF8.GetString(data) : "(null)";
        Console.WriteLine($"Consumed event of type {context.Event.Type} with data: {dataString}");
        await context.ConfirmAsync(cancellationToken);
    }
}