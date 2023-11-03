using Amazon.SQS.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using AmazonSqsSubscription.Config;
using AmazonSqsSubscription.Exceptions;
using AmazonSqsSubscription.Subscription;
using Xunit;

namespace AmazonSqsSubscription.Test
{
    public class SqsConsumerHostedServiceTests : MockStrictBehaviorTest
    {
        private readonly SqsConsumerHostedService _service;
        private readonly Mock<ISqsMessageProcessor> _messageProcessorMock;
        private readonly Mock<ISqsClient> _sqsClientMock;
        private readonly SqsSubscriptionConfig _config;

        public SqsConsumerHostedServiceTests()
        {
            _sqsClientMock = _mockRepository.Create<ISqsClient>();
            _messageProcessorMock = _mockRepository.Create<ISqsMessageProcessor>();
            _config = new SqsSubscriptionConfig
            {
                QueueName = "testQueue", QueueLongPollTimeSeconds = 30
            };

            _service = new SqsConsumerHostedService(
                _sqsClientMock.Object,
                new List<ISqsMessageProcessor> { _messageProcessorMock.Object },
                new NullLogger<SqsConsumerHostedService>(),
                _config);
        }

        [Fact]
        public async Task Ctor_ConfigNotFound_ThrowsException()
        {
            var ex = Assert.Throws<SqsConfigurationException>(() => new SqsConsumerHostedService(null, null, null, null));

            Assert.Equal("_sqsSubscriptionConfig is null!", ex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ProcessesMessagesCorrectly()
        {
            var messageType = "configuration_changed";
            var message = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "MessageType", new MessageAttributeValue { StringValue = messageType } }
                },
                ReceiptHandle = "receipt_handle"
            };

            _sqsClientMock
                .Setup(x => x.ReceiveMessagesAsync(_config.QueueName, _config.QueueLongPollTimeSeconds, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Message> { message });

            _messageProcessorMock
                .Setup(x => x.CanProcess(messageType))
                .Returns(true);

            _messageProcessorMock
                .Setup(x => x.ProcessAsync(message))
                .Returns(Task.CompletedTask);

            _sqsClientMock
                .Setup(x => x.DeleteMessageAsync(_config.QueueName, message.ReceiptHandle, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);


            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(100);

            await _service.StartAsync(cancellationTokenSource.Token);
        }

        [Fact]
        public async Task ExecuteAsync_NoMessageType_HandlesErrorsCorrectly()
        {
            var message = new Message
            {
                ReceiptHandle = "receipt_handle"
            };

            _sqsClientMock
                .Setup(x => x.ReceiveMessagesAsync(_config.QueueName, _config.QueueLongPollTimeSeconds, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Message> { message });

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(500);
            await _service.StartAsync(cancellationTokenSource.Token);
        }

        [Fact]
        public async Task ExecuteAsync_ProcessorNotFound_HandlesErrorsCorrectly()
        {
            var messageType = "configuration_changed";
            var message = new Message
            {
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "MessageType", new MessageAttributeValue { StringValue = messageType } }
                },
                ReceiptHandle = "receipt_handle"
            };

            _sqsClientMock
                .Setup(x => x.ReceiveMessagesAsync(_config.QueueName, _config.QueueLongPollTimeSeconds, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Message> { message });

            _messageProcessorMock
                .Setup(x => x.CanProcess(messageType))
                .Returns(false);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(500);
            await _service.StartAsync(cancellationTokenSource.Token);
        }

        [Fact]
        public async Task ExecuteAsync_MessagesNotReceived_HandlesErrorsCorrectly()
        {
            _sqsClientMock
                .Setup(x => x.ReceiveMessagesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(100);
            await _service.StartAsync(cancellationTokenSource.Token);
        }
    }
}
