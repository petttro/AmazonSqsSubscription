using Amazon.Runtime.Internal;
using Amazon.SQS;
using Microsoft.Extensions.Logging;

namespace AmazonSqsSubscription.Client;

internal class AmazonSQSClientWithRetryLogging : AmazonSQSClient
{
    private readonly ILoggerFactory _loggerFactory;

    public AmazonSQSClientWithRetryLogging(ILoggerFactory loggerFactory, AmazonSQSConfig clientConfig)
        : base(clientConfig)
    {
        _loggerFactory = loggerFactory;
        CustomizeRuntimePipeline();
    }

    private void CustomizeRuntimePipeline()
    {
        RuntimePipeline.ReplaceHandler<RetryHandler>(new RetryHandler(new AmazonSQSRetryPolicyLogger(_loggerFactory, Config)));

        foreach (var handler in RuntimePipeline.Handlers)
        {
            handler.Logger = new AwsLoggerWrapper(_loggerFactory.CreateLogger<AmazonSQSClientWithRetryLogging>());
        }
    }
}
