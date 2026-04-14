using Egov.Integrations.MConnect.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSystemCertificate(builder.Configuration.GetSection("Certificate"));
builder.Services.AddCloudEventsConsumer<Consumer>(builder.Configuration.GetSection("CloudEventsConsumer"));

var app = builder.Build();
app.Run();

class TestEventData
{
    public int Property { get; set; }
}

class Consumer : ICloudEventsConsumer
{
    public async Task ConsumeAsync(CloudEventConsumerContext context, CancellationToken cancellationToken)
    {
        var cloudEvent = context.Event;
        switch (cloudEvent.Type)
        {
            case "test":
                var eventData = cloudEvent.DeserializeData<TestEventData>();
                if (eventData?.Property % 100 == 0)
                {
                    Console.WriteLine($"Consumed event of type {cloudEvent.Type} with property = {eventData?.Property}");
                }
                break;
            default:
                Console.WriteLine($"Consumed event of type {cloudEvent.Type} with data: {cloudEvent.Data}");
                break;
        }

        await context.ConfirmAsync(cancellationToken);
    }
}