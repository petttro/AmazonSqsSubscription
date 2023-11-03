using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using AmazonSqsSubscription.Client;
using Microsoft.Extensions.Logging;

namespace AmazonSqsSubscription;

internal class SqsClient : ISqsClient
{
    private readonly IAmazonSQS _amazonSqs;
    private readonly ILogger<SqsClient> _logger;

    private readonly ConcurrentDictionary<string, string> _queueUrlsCache;

    public SqsClient(IAmazonSQS amazonSqs, ILoggerFactory loggerFactory)
    {
        _amazonSqs = amazonSqs;
        _logger = loggerFactory.CreateLogger<SqsClient>();
        _queueUrlsCache = new ConcurrentDictionary<string, string>();
    }

    public async Task WriteAsync(string queueName, string message, Dictionary<string, string> messageAttributes)
    {
        var queueUrl = await GetQueueUrlAsync(queueName);

        if (string.IsNullOrEmpty(queueUrl))
        {
            return;
        }

        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message));
        }

        _logger.LogInformation($"Sending Message={message} to Qu{queueUrl} queue");

        var timer = new Stopwatch();

        try
        {
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = message,
                MessageAttributes = CreateMessageAttributes(messageAttributes)
            };

            timer.Start();

            var response = await _amazonSqs.SendMessageAsync(request);

            timer.Stop();

            _logger.LogInformation($"Response from SQS={response.HttpStatusCode}, MessageId={response.MessageId}, Duration={timer.ElapsedMilliseconds}");
        }
        catch (Exception ex)
        {
            timer.Stop();

            switch (ex)
            {
                case AmazonServiceException:
                case AmazonClientException:
                case TimeoutException:
                    // if it gets here it means AWS .NET SDK has exhausted all retries and failed
                    _logger.LogError($"Failed to send message to QueueUrl={queueUrl}, Duration={timer.ElapsedMilliseconds}");
                    break;
                default:
                    _logger.LogError(ex, $"Unexpected exception occurred, Duration={timer.ElapsedMilliseconds}");
                    break;
            }
        }
    }

    public async Task<List<Message>> ReceiveMessagesAsync(string queueName, int queueLongPollTimeSeconds, CancellationToken ct)
    {
        var queueUrl = await GetQueueUrlAsync(queueName);

        try
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                WaitTimeSeconds = queueLongPollTimeSeconds,
                AttributeNames = new List<string> { "ApproximateReceiveCount" },
                MessageAttributeNames = new List<string> { "All" }
            };

            var response = await _amazonSqs.ReceiveMessageAsync(request, ct);
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new AmazonSQSException($"Failed to GetMessagesAsync for QueueName={queueName}. ResponseStatusCode={response.HttpStatusCode}");
            }

            return response.Messages;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, $"Failed to GetMessagesAsync for queue QueueName={queueName} because the task was canceled");
            return new List<Message>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to GetMessagesAsync for QueueName={queueName}");
            throw;
        }
    }

    public async Task DeleteMessageAsync(string queueName, string receiptHandle, CancellationToken ct)
    {
        var queueUrl = await GetQueueUrlAsync(queueName);

        try
        {
            var response = await _amazonSqs.DeleteMessageAsync(queueUrl, receiptHandle, ct);
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new AmazonSQSException($"Failed to DeleteMessageAsync with for ReceiptHandle={receiptHandle} from QueueName={queueName}. " +
                                             $"ResponseCode={response.HttpStatusCode}");
            }
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to DeleteMessageAsync from QueueName={queueName}");
            throw;
        }
    }

    public async Task<SqsStatus> GetQueueStatusAsync(string queueName)
    {
        var queueUrl = await GetQueueUrlAsync(queueName);

        try
        {
            var attributes = new List<string> { "ApproximateNumberOfMessages", "ApproximateNumberOfMessagesNotVisible", "LastModifiedTimestamp" };
            var response = await _amazonSqs.GetQueueAttributesAsync(new GetQueueAttributesRequest(queueUrl, attributes));

            return new SqsStatus
            {
                IsHealthy = response.HttpStatusCode == HttpStatusCode.OK,
                Region = _amazonSqs.Config.RegionEndpoint.SystemName,
                QueueName = queueName,
                QueueUrl = MaskAwsAccountNumber(queueUrl),
                ApproximateNumberOfMessages = response.ApproximateNumberOfMessages,
                ApproximateNumberOfMessagesNotVisible = response.ApproximateNumberOfMessagesNotVisible,
                LastModifiedTimestamp = response.LastModifiedTimestamp
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to GetNumberOfMessages for QueueName={queueName}");
            throw;
        }
    }

    private async Task<string> GetQueueUrlAsync(string queueName)
    {
        if (_queueUrlsCache.ContainsKey(queueName))
        {
            return _queueUrlsCache[queueName];
        }

        if (string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentNullException(nameof(queueName));
        }

        _logger.LogInformation($"Checking if {queueName} exists");

        var request = new GetQueueUrlRequest(queueName);
        var response = await _amazonSqs.GetQueueUrlAsync(request);

        _queueUrlsCache.GetOrAdd(queueName, response.QueueUrl);

        return _queueUrlsCache[queueName];
    }

    private Dictionary<string, MessageAttributeValue> CreateMessageAttributes(Dictionary<string, string> stringMessageAttributes)
    {
        if (stringMessageAttributes == null)
        {
            return null;
        }

        var messageAttributes = new Dictionary<string, MessageAttributeValue>();
        foreach (var attribute in stringMessageAttributes)
        {
            var messageAttributeValue = new MessageAttributeValue
            {
                DataType = nameof(String),
                StringValue = attribute.Value
            };

            messageAttributes.Add(attribute.Key, messageAttributeValue);
        }

        return messageAttributes;
    }

    private string MaskAwsAccountNumber(string url)
    {
        if (!string.IsNullOrEmpty(url) && url.Contains("com/"))
        {
            var index = url.IndexOf("com/") + 4;
            var length = url.IndexOf("/", index) - index;
            var accountNumber = url.Substring(index, length);
            var maskedAccountNumber = new string('x', length);

            return url.Replace(accountNumber, maskedAccountNumber);
        }

        return url;
    }
}
