using System;
using Microsoft.Extensions.Logging;
using ILogger = Amazon.Runtime.Internal.Util.ILogger;
using MLogger = Microsoft.Extensions.Logging.ILogger;

namespace AmazonSqsSubscription.Client;

internal class AwsLoggerWrapper : ILogger
{
    private readonly MLogger _logger;

    public AwsLoggerWrapper(MLogger logger)
    {
        _logger = logger;
    }

    public void InfoFormat(string messageFormat, params object[] args)
    {
        _logger.LogInformation(messageFormat, args);
    }

    public void Debug(Exception exception, string messageFormat, params object[] args)
    {
        _logger.LogDebug(exception, messageFormat, args);
    }

    public void DebugFormat(string messageFormat, params object[] args)
    {
        _logger.LogDebug(messageFormat, args);
    }

    public void Error(Exception exception, string messageFormat, params object[] args)
    {
        _logger.LogError(exception, messageFormat, args);
    }

    public void Flush()
    {
    }
}
