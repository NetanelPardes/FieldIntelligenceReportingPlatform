using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Consumer.Models;
using Microsoft.Extensions.Logging;

namespace Consumer.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IReportValidationService _validationService;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _topic;

    public KafkaConsumerService(string bootstrapServers,string topic,string groupId,IReportValidationService validationService,IElasticsearchService elasticsearchService,ILogger<KafkaConsumerService> logger)
    {
        _topic = topic;
        _validationService = validationService;
        _elasticsearchService = elasticsearchService;
        _logger = logger;

        ConsumerConfig configuration = new()
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer =new ConsumerBuilder<string, string>(configuration).Build();
        _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);
        _logger.LogInformation("Consumer is listening to topic {Topic}",_topic);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> result =_consumer.Consume(cancellationToken);
                await ProcessMessageAsync(result.Message.Value,cancellationToken);
                _consumer.Commit(result);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation( "Consumer stopped");
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(string json,CancellationToken cancellationToken)
    {
        try
        {
            FieldReport? report =JsonSerializer.Deserialize<FieldReport>(json,_jsonOptions);
            if (report is null)
            {
                _logger.LogWarning( "Report rejected: JSON is empty");
                return;
            }

            bool isValid = _validationService.IsValid( report,json,out string errorMessage);

            if (!isValid)
            {
                _logger.LogWarning("Report {ReportId} rejected: {ErrorMessage}",report.ReportId,errorMessage);
                return;
            }
            bool alreadyExists = await _elasticsearchService.ReportExistsAsync(report.ReportId!,cancellationToken);
            if (alreadyExists)
            {
                _logger.LogWarning("Duplicate report rejected: {ReportId}",report.ReportId);
                return;
            }

            FieldReportDocument document = new()
            {
                ReportId = report.ReportId!,
                Timestamp = report.Timestamp!.Value,
                AgentId = report.AgentId!,
                Unit = report.Unit!,
                Theater = report.Theater!,
                Sector = report.Sector!,
                Location = report.Location!,
                ReportType = report.ReportType!.Value,
                Priority = report.Priority!.Value,
                SourceType = report.SourceType!,
                Message = report.Message!,
                SubjectId = report.SubjectId,
                SubjectType = report.SubjectType,
                ProcessedAt = DateTimeOffset.UtcNow
            };

            await _elasticsearchService.SaveReportAsync(document,cancellationToken);
            _logger.LogDebug("Report {ReportId} saved",report.ReportId);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning("Report rejected because the JSON is invalid: {ErrorMessage}", exception.Message);
        }
    }
}