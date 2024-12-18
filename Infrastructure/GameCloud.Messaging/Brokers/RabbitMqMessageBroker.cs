using GameCloud.Application.Common.Interfaces;
using RabbitMQ.Client;

namespace GameCloud.Messaging.Brokers;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IConnectionFactory _connectionFactory;

    public RabbitMqEventPublisher(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task PublishAsync<T>(T eventMessage) where T : class
    {
    }
}