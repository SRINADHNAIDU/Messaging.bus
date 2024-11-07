namespace Messaging.bus.MQ;

public interface IRabbitMQPublisher<T>
{
    Task PublishMessageAsync(T message, string queueName);
}
