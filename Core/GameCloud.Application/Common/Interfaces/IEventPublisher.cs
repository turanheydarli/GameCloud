namespace GameCloud.Application.Common.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T eventMessage) where T : class;
}