using Microsoft.AspNetCore.HttpOverrides;
using AmazonSqsSubscription;

namespace Package.Demo;

public class Startup
{
    private readonly ILoggerFactory _loggerFactory;

    public Startup(IConfiguration configuration, ILoggerFactory loggerFactory, IHostEnvironment hostEnvironment)
    {
        Configuration = configuration;
        _loggerFactory = loggerFactory;
        HostEnvironment = hostEnvironment;
    }

    public IConfiguration Configuration { get; }

    public IHostEnvironment HostEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddOptions();

        services
            .AddSqsClient(Configuration, _loggerFactory)
            .AddSqsSubscription(Configuration, "TestSqsSubscription")
            .AddSqsMessageProcessor<TestSqsMessageProcessor>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Startup>();

        logger.LogInformation("Configuring...");

        app.UseRouting();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
