using System;

namespace AmazonSqsSubscription.Config;

/// <summary>
/// Represents the configuration needed for subscribing to an SQS queue.
/// </summary>
public class SqsSubscriptionConfig
{
    /// <summary>
    /// Gets or sets the name of the SQS queue to subscribe to.
    /// </summary>
    public string QueueName { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds to wait for a message to become available
    /// while polling the SQS queue.
    /// </summary>
    public int QueueLongPollTimeSeconds { get; set; }
}
