using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace AmazonSqsSubscription.Subscription;

/// <summary>
/// Defines a contract for processing SQS messages.
/// </summary>
public interface ISqsMessageProcessor
{
    /// <summary>
    /// Determines whether the processor can handle a specific message type.
    /// </summary>
    /// <param name="messageType">The type of the message to check.</param>
    /// <returns>True if the processor can handle the message type; otherwise, false.</returns>
    bool CanProcess(string messageType);

    /// <summary>
    /// Processes the specified message asynchronously.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <returns>A task that represents the asynchronous processing operation.</returns>
    Task ProcessAsync(Message message);
}
