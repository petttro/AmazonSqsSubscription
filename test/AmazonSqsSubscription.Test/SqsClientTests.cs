using System.Net;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AmazonSqsSubscription.Test;

public class SqsClientTests : MockStrictBehaviorTest
{
    private readonly SqsClient _sqsClient;
    private readonly Mock<IAmazonSQS> _amazonSqsMock;

    public SqsClientTests()
    {
        _amazonSqsMock = _mockRepository.Create<IAmazonSQS>();
        _sqsClient = new SqsClient(_amazonSqsMock.Object, new NullLoggerFactory());
    }

    [Fact]
    public async Task WriteAsync_Successful()
    {
        // Arrange
        var queueName = "testQueue";
        var message = "testMessage";
        var attributes = new Dictionary<string, string>() { { "Key", "Value" } };
        var sendMessageResponse = new SendMessageResponse { HttpStatusCode = HttpStatusCode.OK };

        _amazonSqsMock
            .Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock
            .Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default))
            .ReturnsAsync(sendMessageResponse);

        // Act
        await _sqsClient.WriteAsync(queueName, message, attributes);

        // Assert
        _amazonSqsMock.Verify(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default), Times.Once);
    }


    [Fact]
    public async Task WriteAsync_NullOrEmptyMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var queueName = "testQueue";
        string message = null;

        _amazonSqsMock
            .Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });


        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sqsClient.WriteAsync(queueName, message, new Dictionary<string, string>()));
    }

    [Fact]
    public async Task ReceiveMessagesAsync_Successful()
    {
        // Arrange
        var queueName = "testQueue";
        var messages = new List<Message> { new Message { MessageId = "1" } };

        var receiveMessageResponse = new ReceiveMessageResponse
        {
            Messages = messages,
            HttpStatusCode = HttpStatusCode.OK
        };

        _amazonSqsMock.Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), default))
            .ReturnsAsync(receiveMessageResponse);

        // Act
        var result = await _sqsClient.ReceiveMessagesAsync(queueName, 10, default);

        // Assert
        Assert.Equal(messages, result);
    }

    [Fact]
    public async Task ReceiveMessagesAsync_TaskCanceledException_ReturnsEmptyList()
    {
        // Arrange
        var queueName = "testQueue";

        _amazonSqsMock.Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), default))
            .ThrowsAsync(new TaskCanceledException());

        // Act
        var result = await _sqsClient.ReceiveMessagesAsync(queueName, 10, default);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReceiveMessagesAsync_ThrowsAmazonSQSException()
    {
        // Arrange
        var queueName = "testQueue";

        _amazonSqsMock.Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), default))
            .ThrowsAsync(new AmazonSQSException("An error occurred"));

        // Act & Assert
        await Assert.ThrowsAsync<AmazonSQSException>(() => _sqsClient.ReceiveMessagesAsync(queueName, 10, default));
    }

    [Fact]
    public async Task ReceiveMessagesAsync_UnexpectedException_ThrowsException()
    {
        // Arrange
        var queueName = "testQueue";

        _amazonSqsMock.Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), default))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sqsClient.ReceiveMessagesAsync(queueName, 10, default));
    }

    [Fact]
    public async Task DeleteMessageAsync_Successful()
    {
        // Arrange
        var queueName = "testQueue";
        var receiptHandle = "receiptHandle";
        var deleteMessageResponse = new DeleteMessageResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK
        };

        _amazonSqsMock.Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), receiptHandle, default))
            .ReturnsAsync(deleteMessageResponse);

        // Act
        await _sqsClient.DeleteMessageAsync(queueName, receiptHandle, default);

        // Assert (No exception thrown)
    }

    [Fact]
    public async Task DeleteMessageAsync_ThrowsAmazonSQSException()
    {
        // Arrange
        var queueName = "testQueue";
        var receiptHandle = "receiptHandle";

        _amazonSqsMock.Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), receiptHandle, default))
            .ThrowsAsync(new AmazonSQSException("An error occurred"));

        // Act & Assert
        await Assert.ThrowsAsync<AmazonSQSException>(() => _sqsClient.DeleteMessageAsync(queueName, receiptHandle, default));
    }

    [Fact]
    public async Task DeleteMessageAsync_UnexpectedException_ThrowsException()
    {
        // Arrange
        var queueName = "testQueue";
        var receiptHandle = "receiptHandle";

        _amazonSqsMock.Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock.Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), receiptHandle, default))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sqsClient.DeleteMessageAsync(queueName, receiptHandle, default));
    }

    [Fact]
    public async Task GetQueueStatusAsync_Successful()
    {
        // Arrange
        var testQueue = "testQueue";
        var queueUrl = "http://testQueueUrl";

        _amazonSqsMock.SetupGet(s => s.Config).Returns(new AmazonSQSConfig
        {
            RegionEndpoint = RegionEndpoint.USEast1 // or any other region you prefer
        });

        _amazonSqsMock
            .Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = queueUrl });

        _amazonSqsMock
            .Setup(s => s.GetQueueAttributesAsync(It.IsAny<GetQueueAttributesRequest>(), default))
            .ReturnsAsync(new GetQueueAttributesResponse
            {
                HttpStatusCode = HttpStatusCode.OK,
                Attributes = new Dictionary<string, string>
                {
                    { "ApproximateNumberOfMessages", "10" },
                    { "ApproximateNumberOfMessagesNotVisible", "0" },
                    { "LastModifiedTimestamp", "1677648476" }
                }
            });

        // Act
        var status = await _sqsClient.GetQueueStatusAsync(testQueue);

        // Assert
        Assert.True(status.IsHealthy);
        Assert.Equal(10, status.ApproximateNumberOfMessages);
        Assert.Equal(0, status.ApproximateNumberOfMessagesNotVisible);
        Assert.Equal(queueUrl, status.QueueUrl);
        Assert.Equal(testQueue, status.QueueName);
        Assert.Equal("us-east-1", status.Region);
        Assert.True(DateTime.Now > status.LastModifiedTimestamp);
    }

    [Fact]
    public async Task GetQueueStatusAsync_ThrowsException()
    {
        // Arrange
        var testQueue = "testQueue";

        _amazonSqsMock.Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock.Setup(s => s.GetQueueAttributesAsync(It.IsAny<GetQueueAttributesRequest>(), default))
            .Throws(new AmazonSQSException("An error occurred"));

        // Act & Assert
        await Assert.ThrowsAsync<AmazonSQSException>(() => _sqsClient.GetQueueStatusAsync(testQueue));
    }

    [Fact]
    public async Task GetQueueStatusAsync_FailedResponse()
    {
        // Arrange
        var testQueue = "testQueue";

        _amazonSqsMock.SetupGet(s => s.Config).Returns(new AmazonSQSConfig
        {
            RegionEndpoint = RegionEndpoint.USEast1 // or any other region you prefer
        });

        _amazonSqsMock.Setup(s => s.GetQueueUrlAsync(It.IsAny<GetQueueUrlRequest>(), default))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = "http://testQueueUrl" });

        _amazonSqsMock.Setup(s => s.GetQueueAttributesAsync(It.IsAny<GetQueueAttributesRequest>(), default))
            .ReturnsAsync(new GetQueueAttributesResponse { HttpStatusCode = HttpStatusCode.BadRequest });

        // Act
        var status = await _sqsClient.GetQueueStatusAsync(testQueue);

        // Assert
        Assert.False(status.IsHealthy);
    }
}
