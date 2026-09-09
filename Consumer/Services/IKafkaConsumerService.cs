namespace Consumer.Services;

public interface IKafkaConsumerService
{
    Task StartConsumingAsync(
        CancellationToken cancellationToken
    );
}