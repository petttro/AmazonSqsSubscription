using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using AmazonSqsSubscription.Client;

namespace AmazonSqsSubscription;

/// <summary>
/// Defines contract for an SQS client to interact with SQS queues.
/// </summary>
public interface ISqsClient
{
    /// <summary>
    /// Writes a message to the specified SQS queue asynchronously.
    /// </summary>
    /// <param name="queueName">The name of the queue to write the message to.</param>
    /// <param name="messageBody">The body of the message to be written to the queue.</param>
    /// <param name="messageAttributes">The message string value attributes to write to the message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteAsync(string queueName, string messageBody, Dictionary<string, string> messageAttributes);

    /// <summary>
    /// Receives messages from the specified SQS queue asynchronously.
    /// </summary>
    /// <param name="queueName">The name of the queue to receive messages from.</param>
    /// <param name="queueLongPollTimeSeconds">The duration in seconds to wait for a message to become available.</param>
    /// <param name="ct">Cancellation token to cancel the receive messages request.</param>
    /// <returns>A task that represents the asynchronous receive operation, containing a list of messages.</returns>
    Task<List<Message>> ReceiveMessagesAsync(string queueName, int queueLongPollTimeSeconds, CancellationToken ct);

    /// <summary>
    /// Deletes a message from the specified SQS queue asynchronously.
    /// </summary>
    /// <param name="queueName">The name of the queue to delete the message from.</param>
    /// <param name="receiptHandle">The receipt handle associated with the message to delete.</param>
    /// <param name="ct">Cancellation token to cancel the delete message request.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteMessageAsync(string queueName, string receiptHandle, CancellationToken ct);

    /// <summary>
    /// Gets the status of the specified SQS queue asynchronously.
    /// </summary>
    /// <param name="queueName">The name of the queue to get the status of.</param>
    /// <returns>A task that represents the asynchronous status retrieval operation, containing the queue status.</returns>
    Task<SqsStatus> GetQueueStatusAsync(string queueName);
}
