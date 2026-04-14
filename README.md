# Egov.Integrations.MConnect.Events

[![NuGet Version](https://img.shields.io/nuget/v/Egov.Integrations.MConnect.Events.svg)](https://www.nuget.org/packages/Egov.Integrations.MConnect.Events)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

This library facilitates the integration of .NET applications with MConnect Events, providing easy-to-use producers and consumers for CloudEvents. It simplifies the process of producing and consuming events by providing a fluent API, background services, and seamless integration with .NET dependency injection.

---

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Packages](#packages)
- [Configuration](#configuration)
- [Usage](#usage)
  - [Registering Producer](#registering-producer)
  - [Registering Consumer and Handlers](#registering-consumer-and-handlers)
  - [Implementing a Handler](#implementing-a-handler)
- [Testing](#testing)
- [Contributing](#contributing)
- [Code of Conduct](#code-of-conduct)
- [AI Assistance](#ai-assistance)
- [License](#license)

---

## Features

- **CloudEvents Support**: Built on the [CloudEvents](https://cloudevents.io/) specification (version 1.0).
- **Producer & Consumer**: High-level abstractions for sending and receiving events.
- **Background Services**: Automatic event consumption via hosted services.
- **Typed Handlers**: Support for generic handlers with strongly-typed JSON data.
- **Flexible Configuration**: Integration with `IConfiguration` and `IServiceCollection`.
- **Security**: Built-in support for system certificates and mutual TLS (mTLS).

---

## Prerequisites

- .NET 10.0 or later
- MConnect Events service access

---

## Installation

Install the package via NuGet:

```bash
dotnet add package Egov.Integrations.MConnect.Events
```

---

## Packages

- **Egov.Integrations.MConnect.Events**: The main library containing producer, consumer, and handler abstractions.

---

## Configuration

Add the necessary configuration to your **appsettings.json**:

```json
{
  "MConnect": {
    "Producer": {
      "BaseAddress": "https://mconnect.example.com/",
      "Timeout": "00:01:40"
    },
    "Consumer": {
      "BaseAddress": "https://mconnect.example.com/",
      "ConsumeEvents": true,
      "ConsumeTest": true,
      "ConsumeDead": false
    }
  }
}
```

---

## Usage

### Registering Producer

Register the producer in your **Program.cs**:

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add MConnect Events producer
builder.Services.AddCloudEventsProducer(builder.Configuration.GetSection("MConnect:Producer"));

var app = builder.Build();

// Usage in a service
public class MyService(ICloudEventsProducer producer)
{
    public async Task SendEventAsync()
    {
        var cloudEvent = new CloudEvent
        {
            Id = Guid.NewGuid().ToString(),
            Source = new Uri("https://my-service.gov.md"),
            Type = "md.gov.egov.order.created",
            Data = new { OrderId = 123 }
        };
        await producer.ProduceAsync(cloudEvent);
    }
}
```

### Registering Consumer and Handlers

To consume events, register handlers using the fluent builder:

```csharp
builder.Services.AddCloudEventHandlers(builder.Configuration.GetSection("MConnect:Consumer"))
    .AddSingletonHandler<MySimpleHandler>("md.gov.egov.order.*")
    .AddScopedHandler<MyTypedHandler, OrderData>("md.gov.egov.payment.completed");
```

### Implementing a Handler

Implement `IHandleCloudEvents` for generic handling or `IHandleCloudEvents<TData>` for typed data:

```csharp
public class MySimpleHandler : IHandleCloudEvents
{
    public async Task HandleAsync(CloudEventConsumerContext context, CancellationToken cancellationToken)
    {
        var cloudEvent = context.Event;
        // Process event...
        
        await context.ConfirmAsync(cancellationToken);
    }
}

public record OrderData(int OrderId);

public class MyTypedHandler : IHandleCloudEvents<OrderData>
{
    public async Task HandleAsync(CloudEventConsumerContext context, OrderData data, CancellationToken cancellationToken)
    {
        // Process typed data...
        await context.ConfirmAsync(cancellationToken);
    }
}
```

---

## Testing

The solution includes a test suite using xUnit.

### Running the tests

```bash
dotnet test
```

---

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to get started.

---

## Code of Conduct

This project adheres to the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

---

## AI Assistance

This repository contains an [AGENTS.md](AGENTS.md) file with instructions and context for AI coding agents to assist in development, ensuring consistency in code style and project structure.

---

## License

This project is licensed under the [MIT License](LICENSE).
