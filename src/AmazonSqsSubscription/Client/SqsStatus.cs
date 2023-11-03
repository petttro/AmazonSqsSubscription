using System;

namespace AmazonSqsSubscription.Client;

/// <summary>
/// Represents the status of an SQS queue.
/// </summary>
public class SqsStatus
{
    /// <summary>
    /// Gets or sets a value indicating whether the queue is healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the region where the SQS queue is hosted.
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    /// Gets or sets the name of the SQS queue.
    /// </summary>
    public string QueueName { get; set; }

    /// <summary>
    /// Gets or sets the URL of the SQS queue.
    /// </summary>
    public string QueueUrl { get; set; }

    /// <summary>
    /// Gets or sets the approximate number of messages available for retrieval from the queue.
    /// </summary>
    public int ApproximateNumberOfMessages { get; set; }

    /// <summary>
    /// Gets or sets the approximate number of messages that are not visible to consumers.
    /// </summary>
    public int ApproximateNumberOfMessagesNotVisible { get; set; }

    /// <summary>
    /// Gets or sets the last modified timestamp of the SQS queue.
    /// </summary>
    public DateTime LastModifiedTimestamp { get; set; }
}
