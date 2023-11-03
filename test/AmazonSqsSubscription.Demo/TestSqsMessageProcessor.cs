using Amazon.SQS.Model;
using Package.Demo.Extensions;
using AmazonSqsSubscription.Subscription;

namespace Package.Demo;

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
