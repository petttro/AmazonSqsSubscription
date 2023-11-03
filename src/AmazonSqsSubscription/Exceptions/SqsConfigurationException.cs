using System;

namespace AmazonSqsSubscription.Exceptions;

/// <summary>
/// Represents errors that occur during SQS configuration.
/// </summary>
public class SqsConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqsConfigurationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SqsConfigurationException(string message)
        : base(message)
    {
    }
}
