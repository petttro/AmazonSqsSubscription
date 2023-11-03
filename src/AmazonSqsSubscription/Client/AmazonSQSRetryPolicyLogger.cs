using System;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Microsoft.Extensions.Logging;
using AmazonSqsSubscription.Extensions;

namespace AmazonSqsSubscription.Client;

internal class AmazonSQSRetryPolicyLogger : DefaultRetryPolicy
{
    private readonly ILogger<AmazonSQSRetryPolicyLogger> _logger;

    public AmazonSQSRetryPolicyLogger(ILoggerFactory loggerFactory, IClientConfig config)
        : base(config)
    {
        _logger = loggerFactory.CreateLogger<AmazonSQSRetryPolicyLogger>();

        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }
    }

    public override bool OnRetry(IExecutionContext executionContext)
    {
        LogRetry(executionContext);

        return base.OnRetry(executionContext);
    }

    public override bool OnRetry(IExecutionContext executionContext, bool bypassAcquireCapacity)
    {
        LogRetry(executionContext);

        return base.OnRetry(executionContext, bypassAcquireCapacity);
    }

    private void LogRetry(IExecutionContext executionContext)
    {
        var responseMetadata = executionContext?.ResponseContext?.Response?.ResponseMetadata?.Metadata?.SerializeJsonSafe();
        var responseCode = executionContext?.ResponseContext?.Response?.HttpStatusCode;
        var requestParams = executionContext?.RequestContext?.Request?.Parameters?.SerializeJsonSafe();

        _logger.LogTrace(
            $"RequestName={executionContext?.RequestContext?.RequestName} RetriesCount={executionContext?.RequestContext?.Retries} " +
            $"MaxRetries={MaxRetries} RequestParameters={requestParams} ResponseCode={responseCode} ResponseMetadata={responseMetadata}");
    }
}
