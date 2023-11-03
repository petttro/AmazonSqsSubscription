using System;
using System.Collections.Generic;
using Amazon.SQS;
using AmazonSqsSubscription.Client;
using AmazonSqsSubscription.Config;
using AmazonSqsSubscription.Exceptions;
using AmazonSqsSubscription.Subscription;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AmazonSqsSubscription;

/// <summary>
/// Provides extension methods to register and configure SQS services and processors.
/// </summary>
public static class AddSqsExtensions
{
    /// <summary>
    /// Adds and configures an SQS subscription to the specified SQS queue.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the subscription to.</param>
    /// <param name="configuration">The application's configuration.</param>
    /// <param name="subscriptionConfigSectionName">The name of the configuration section for the SQS subscription.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSqsSubscription(
        this IServiceCollection services, IConfiguration configuration, string subscriptionConfigSectionName)
    {
        if (string.IsNullOrEmpty(subscriptionConfigSectionName))
        {
            throw new SqsConfigurationException($"SectionName for {nameof(SqsSubscriptionConfig)} should be defined!");
        }

        var sqsSubscriptionConfig = configuration.GetSection(subscriptionConfigSectionName).Get<SqsSubscriptionConfig>();
        if (sqsSubscriptionConfig == null)
        {
            throw new SqsConfigurationException($"{nameof(SqsSubscriptionConfig)} must be defined in IConfiguration!");
        }

        services.AddSingleton<IHostedService>(sp => new SqsConsumerHostedService(
            sp.GetRequiredService<ISqsClient>(),
            sp.GetRequiredService<IEnumerable<ISqsMessageProcessor>>(),
            sp.GetRequiredService<ILogger<SqsConsumerHostedService>>(),
            sqsSubscriptionConfig));

        return services;
    }

    /// <summary>
    /// Registers Amazon SQS services <see cref="IAmazonSQS"/>, <see cref="ISqsClient"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application's configuration.</param>
    /// <param name="loggerFactory">The factory to create loggers.</param>
    /// <param name="configSectionName">The configuration section name for <see cref="AmazonSQSConfig"/> "AmazonSqs" by default.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSqsClient(
        this IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory, string configSectionName = "AmazonSqs")
    {
        var amazonSqsConfig = configuration.GetSection(configSectionName).Get<AmazonSQSConfig>();
        if (amazonSqsConfig == null)
        {
            throw new SqsConfigurationException($"{nameof(AmazonSQSConfig)} must be defined in IConfiguration!");
        }

        services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClientWithRetryLogging(loggerFactory, amazonSqsConfig));
        services.AddSingleton<ISqsClient, SqsClient>();

        return services;
    }

    /// <summary>
    /// Registers Amazon SQS services including <see cref="IAmazonSQS"/> and <see cref="ISqsClient"/> with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application's configuration where AWS credentials and service settings are specified.</param>
    /// <param name="loggerFactory">The factory used to create loggers for logging purposes.</param>
    /// <param name="amazonSqsConfig">The Amazon SQS configuration settings.
    /// If not provided, the method will use "AmazonSqs" as the default section name from the configuration.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="amazonSqsConfig"/> is null.</exception>
    /// <returns>The same <see cref="IServiceCollection"/> instance to allow for chaining multiple calls.</returns>
    public static IServiceCollection AddSqsClient(
        this IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory, AmazonSQSConfig amazonSqsConfig)
    {
        if (amazonSqsConfig is null)
        {
            throw new ArgumentException($"{nameof(amazonSqsConfig)} required!");
        }

        services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClientWithRetryLogging(loggerFactory, amazonSqsConfig));
        services.AddSingleton<ISqsClient, SqsClient>();

        return services;
    }

    /// <summary>
    /// Registers an SQS message processor in the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the message processor to add.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the processor to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSqsMessageProcessor<T>(this IServiceCollection services)
        where T : class, ISqsMessageProcessor
    {
        return services.AddScoped<ISqsMessageProcessor, T>();
    }
}
