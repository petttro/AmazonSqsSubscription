using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using AmazonSqsSubscription.Config;
using AmazonSqsSubscription.Exceptions;
using AmazonSqsSubscription.Subscription;
using Xunit;

namespace AmazonSqsSubscription.Test;

public class AddSqsExtensionsTests
{
    [Fact]
    public void AddSqsSubscription_RegistersHostedServiceCorrectly()
    {
        var subscribtionConfigSectionName = "SubscriptionConfig";
        var amazonSqsConfigSectionName = "AmazonSqsConfig";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { $"{subscribtionConfigSectionName}:QueueName", "test_queue" },
                { $"{subscribtionConfigSectionName}:QueueLongPollTimeSeconds", "5" },
                { $"{amazonSqsConfigSectionName}:Timeout", "5" },
                { $"{amazonSqsConfigSectionName}:MaxErrorRetry", "3" },
            })
            .Build();

        var services = new ServiceCollection();
        var nullLoggerFactory = new NullLoggerFactory();

        services.AddSingleton<ILogger<SqsConsumerHostedService>>(new NullLogger<SqsConsumerHostedService>());
        services.AddSingleton<ILoggerFactory>(nullLoggerFactory);
        services.AddSqsClient(configuration, nullLoggerFactory, amazonSqsConfigSectionName);
        services.AddSqsSubscription(configuration, subscribtionConfigSectionName);

        var serviceProvider = services.BuildServiceProvider();

        // Asserting that an IHostedService is registered which should be SqsConsumerHostedService
        var hostedService = serviceProvider.GetService<IHostedService>();
        Assert.NotNull(hostedService);
    }

    [Fact]
    public void AddSqsSubscription_SectionNameNotDefined_ThrowException()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<SqsConfigurationException>(() => services.AddSqsSubscription(null, String.Empty));

        Assert.Equal($"SectionName for {nameof(SqsSubscriptionConfig)} should be defined!", ex.Message);
    }

    [Fact]
    public void AddSqsSubscription_ConfigNotDefined_ThrowException()
    {
        var amazonSqsConfigSectionName = "AmazonSqsConfig";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "SomeConfig:ButWrong", "5" },
            })
            .Build();

        var services = new ServiceCollection();

        var ex = Assert.Throws<SqsConfigurationException>(() => services.AddSqsSubscription(configuration, amazonSqsConfigSectionName));

        Assert.Equal($"{nameof(SqsSubscriptionConfig)} must be defined in IConfiguration!", ex.Message);
    }

    [Fact]
    public void AddAmazonSqs_RegistersSqsServicesCorrectly()
    {
        var amazonSqsConfigSectionName = "AmazonSqsConfig";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { $"{amazonSqsConfigSectionName}:Timeout", "5" },
                { $"{amazonSqsConfigSectionName}:MaxErrorRetry", "3" },
            })
            .Build();

        var services = new ServiceCollection();
        var nullLoggerFactory = new NullLoggerFactory();

        services.AddSingleton<ILogger<SqsConsumerHostedService>>(new NullLogger<SqsConsumerHostedService>());
        services.AddSingleton<ILoggerFactory>(nullLoggerFactory);

        services.AddSqsClient(configuration, nullLoggerFactory, amazonSqsConfigSectionName);

        var serviceProvider = services.BuildServiceProvider();

        // Asserting that ISqsClient and IAmazonSQS services are registered
        Assert.NotNull(serviceProvider.GetService<ISqsClient>());
        Assert.NotNull(serviceProvider.GetService<IAmazonSQS>());
    }

    [Fact]
    public void AddAmazonSqs_ConfigNotDefined_ThrowsException()
    {
        var amazonSqsConfigSectionName = "AmazonSqsConfig";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "SomeConfig:ButWrong", "5" },
            })
            .Build();

        var services = new ServiceCollection();

        var ex = Assert.Throws<SqsConfigurationException>(() => services.AddSqsClient(configuration, null, amazonSqsConfigSectionName));

        Assert.Equal($"{nameof(AmazonSQSConfig)} must be defined in IConfiguration!", ex.Message);
    }

    [Fact]
    public void AddSqsMessageProcessor_RegistersMessageProcessorCorrectly()
    {
        var services = new ServiceCollection();

        // Registering a dummy ISqsMessageProcessor
        services.AddSqsMessageProcessor<MockSqsMessageProcessor>();

        var serviceProvider = services.BuildServiceProvider();

        // Asserting that ISqsMessageProcessor is registered and of type MockSqsMessageProcessor
        var processor = serviceProvider.GetService<ISqsMessageProcessor>();
        Assert.NotNull(processor);
        Assert.IsType<MockSqsMessageProcessor>(processor);
    }

    // Dummy processor for testing purposes
    public class MockSqsMessageProcessor : ISqsMessageProcessor
    {
        public bool CanProcess(string messageType) => true;

        public Task ProcessAsync(Message message) => Task.CompletedTask;
    }
}
