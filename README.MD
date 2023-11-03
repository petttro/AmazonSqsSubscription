# AmazonSqsSubscription nuget package

AmazonSqsSubscription is a .NET NuGet package that provides a simple and effective way to interact with Amazon Simple Queue Service (SQS). It offers a
wrapper (`ISqsClient`) around the `IAmazonSQS` interface and enables microservices to subscribe to SQS queues with minimal setup.

## Features

- Easy to set up client interface for Amazon SQS (`ISqsClient`).
- Seamless integration with .NET Core microservices for subscribing to SQS queues.
- Supports multiple SQS subscriptions within the same service.

## Getting Started

### Usage

#### TL;DR

Package included working demo application where you can find fully configured example:
TODO: add link here to startup.cs

### Configuration Example

Add the following configuration to your `appsettings.json`  file, adjusting the queue names and settings as necessary:

```json
{
    "AmazonSqs": {
        "Timeout": "00:00:15.0000000",
        "ThrottleRetries": "true",
        "MaxErrorRetry": "3",
        "LogMetrics": "false"
    },
    "TestSqsSubscription": {
        "QueueName": "petr-sqs-test",
        "QueueLongPollTimeSeconds": 10
    }
}
```

#### In Startup.cs

In your `Startup.cs`, configure the services as follows:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddSqsClient(Configuration, _loggerFactory)
        .AddSqsSubscription(Configuration, "TestSqsSubscription")
        .AddSqsMessageProcessor<TestSqsMessageProcessor>();
}
```
In the provided example, we have:

- Registered `ISqsClient` with the dependency injection (DI) container to facilitate interaction with the Amazon SQS service.
- Added a subscription to the `TestSqsSubscription` SQS queue, allowing our service to receive messages from this queue.
- Registered the `TestSqsMessageProcessor` class with the DI container. This class is responsible for processing messages received from the `TestSqsSubscription` queue (see the example provided below for implementation details).

To process messages received from your SQS subscription, you must create a class named `YourNameSqsMessageProcessor`. This class should implement the `ISqsMessageProcessor` interface provided by this package.

Here's an illustrative example:

```csharp
public class TestSqsMessageProcessor : ISqsMessageProcessor
{
    private readonly ILogger<TestSqsMessageProcessor> _logger;

    public TestSqsMessageProcessor(ILogger<TestSqsMessageProcessor> logger)
    {
        _logger = logger;
    }

    public bool CanProcess(string messageType)
    {
        return messageType == TestMessage.MessageType;
    }

    public async Task ProcessAsync(Message message)
    {
        _logger.LogInformation($"Test Message={message.SerializeJsonSafe()}. Processed");
    }
}
```



### Advanced Configuration

The package allows further customization of the SQS client `Amazon.SQS.AmazonSQSConfig`. Please refer to the API documentation for advanced options.
