namespace JobProcessor.Domain.Interfaces.Services.RabbitMQ;

public interface IRabbitMqService
{
    Task PublishAsync(string queueName, string message);
}