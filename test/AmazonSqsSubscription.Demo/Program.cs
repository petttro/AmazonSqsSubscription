using System.Net;
using Package.Demo.Extensions;
using AmazonSqsSubscription;

namespace Package.Demo;

// https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
// https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-2/
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = new WebHostBuilder()
            .UseKestrel(options => { options.Listen(IPAddress.Any, port: 5000); })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder
                    .AddEnvironmentVariables()
                    .AddJsonFile("appsettings.package.json");

            })
            .UseStartup<Startup>()
            .Build();

        await SendTestMessageToSqsAsync(host.Services);

        host.Run();

        // return Task.CompletedTask;
    }

    private static async Task SendTestMessageToSqsAsync(IServiceProvider sp)
    {
        var sqsClient = sp.GetRequiredService<ISqsClient>();

        var message = new TestMessage
        {
            Action = "Something happened",
            Application = "demo",
            LastUpdateDateTime = DateTime.UtcNow.ToString()
        }.SerializeJsonSafe();

        var attributes = new Dictionary<string, string>()
        {
            { "MessageType", TestMessage.MessageType }
        };

        await sqsClient.WriteAsync("petr-sqs-test", message, attributes);
    }
}
