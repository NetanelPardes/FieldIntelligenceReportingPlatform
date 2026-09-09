using Confluent.Kafka;
using Consumer.Services;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;


Env.TraversePath().Load();

string bootstrapServers =GetRequiredEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");
string topic =GetRequiredEnvironmentVariable("REPORTS_TOPIC");
string groupId =GetRequiredEnvironmentVariable("KAFKA_CONSUMER_GROUP_ID");
string elasticsearchUrl =
    GetRequiredEnvironmentVariable(
        "ELASTICSEARCH_URL"
    );

string elasticsearchIndex =
    GetRequiredEnvironmentVariable(
        "ELASTICSEARCH_INDEX"
    );
ServiceCollection services = new();
string logsDirectory = Path.Combine(Directory.GetCurrentDirectory(),"Logs");
Directory.CreateDirectory(logsDirectory);
string logFilePath = Path.Combine(logsDirectory,"consumer-.log");
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        logFilePath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss} " +
            "[{Level:u3}] {Message:lj}" +
            "{NewLine}{Exception}"
    )
    .CreateLogger();


services.AddLogging(logging =>
{
    logging.ClearProviders();

    logging.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat =
            "yyyy-MM-dd HH:mm:ss ";
    });

    logging.AddSerilog(
        Log.Logger,
        dispose: false
    );
});


services.AddSingleton<
    IReportValidationService,
    ReportValidationService
>();

services.AddSingleton<IElasticsearchService>(
    new ElasticsearchService(
        elasticsearchUrl,
        elasticsearchIndex
    )
);      

services.AddSingleton<IKafkaConsumerService>(
    provider =>
        new KafkaConsumerService(
            bootstrapServers,
            topic,
            groupId,
            provider.GetRequiredService<
                IReportValidationService
            >(),
            provider.GetRequiredService<
                IElasticsearchService
            >(),
            provider.GetRequiredService<
                ILogger<KafkaConsumerService>
            >()
        )
);


using ServiceProvider serviceProvider =
    services.BuildServiceProvider();

IElasticsearchService elasticsearchService =
    serviceProvider.GetRequiredService<
        IElasticsearchService
    >();

IKafkaConsumerService consumerService =
    serviceProvider.GetRequiredService<
        IKafkaConsumerService
    >();

var logger = serviceProvider
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("Consumer");

using CancellationTokenSource cancellationTokenSource =
    new();


Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};


try
{
    logger.LogInformation(
        "Consumer application started"
    );

    await elasticsearchService
    .CreateIndexIfNotExistsAsync(
        cancellationTokenSource.Token
    );

    logger.LogInformation(
        "Elasticsearch index {IndexName} is ready",
        elasticsearchIndex
    );

    await consumerService.StartConsumingAsync(
    cancellationTokenSource.Token
);
}
catch (KafkaException exception)
{
    logger.LogError(
        "Kafka error: {ErrorMessage}",
        exception.Message
    );

    Environment.ExitCode = 1;
}
catch (Exception exception)
{
    logger.LogError(
        "Unexpected consumer error: {ErrorMessage}",
        exception.Message
    );

    Environment.ExitCode = 1;
}
finally
{
    logger.LogInformation(
        "Consumer application finished"
    );

    Log.CloseAndFlush();
}


static string GetRequiredEnvironmentVariable(
    string variableName)
{
    string? value =
        Environment.GetEnvironmentVariable(
            variableName
        );

    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException(
            $"{variableName} is missing from .env"
        );
    }

    return value;
}