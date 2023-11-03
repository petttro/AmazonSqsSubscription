using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using AmazonSqsSubscription.Config;
using AmazonSqsSubscription.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AmazonSqsSubscription.Extensions;

[assembly: InternalsVisibleTo("AmazonSqsSubscription.Test")]

namespace AmazonSqsSubscription.Subscription;

internal class SqsConsumerHostedService : BackgroundService
{
    private readonly string _queueName;
    private readonly ISqsClient _sqsClient;
    private readonly IEnumerable<ISqsMessageProcessor> _messageProcessors;
    private readonly ILogger<SqsConsumerHostedService> _logger;
    private readonly SqsSubscriptionConfig _sqsSubscriptionConfig;

    public SqsConsumerHostedService(
        ISqsClient sqsClient,
        IEnumerable<ISqsMessageProcessor> messageProcessors,
        ILogger<SqsConsumerHostedService> logger,
        SqsSubscriptionConfig sqsSubscriptionConfig)
    {
        _sqsSubscriptionConfig = sqsSubscriptionConfig;
        if (_sqsSubscriptionConfig == null)
        {
            throw new SqsConfigurationException($"{nameof(_sqsSubscriptionConfig)} is null!");
        }

        _queueName = _sqsSubscriptionConfig.QueueName;

        _sqsClient = sqsClient;
        _messageProcessors = messageProcessors;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Starting {nameof(SqsConsumerHostedService)} for QueueName={_queueName}, LongPoolTimeSeconds={_sqsSubscriptionConfig.QueueLongPollTimeSeconds}");
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var messages = await _sqsClient.ReceiveMessagesAsync(_queueName, _sqsSubscriptionConfig.QueueLongPollTimeSeconds, ct);
                var tasks = messages.Select(msg => ProcessMessageAsync(msg, ct));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Queue processing was cancelled");
            }
        }
    }

    private async Task ProcessMessageAsync(Message message, CancellationToken ct)
    {
        try
        {
            var messageType = message.MessageAttributes.GetMessageTypeAttributeValue();
            if (messageType == null)
            {
                throw new Exception($"No 'MessageType' attribute present in Message={message.SerializeJsonSafe()}");
            }

            var processor = _messageProcessors.SingleOrDefault(x => x.CanProcess(messageType));
            if (processor == null)
            {
                throw new Exception($"No processor found for MessageType={messageType}");
            }

            await processor.ProcessAsync(message);
            await _sqsClient.DeleteMessageAsync(_queueName, message.ReceiptHandle, ct);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Cannot process message MessageId={message.MessageId}, ReceiptHandle={message.ReceiptHandle}, " +
                               $"MessageBody={message.Body}, QueueName={_queueName}";
            _logger.LogError(ex, errorMessage);
        }
    }
}
