using TrainerApi.Events;

namespace TrainerApi.Infrastructure.Producers;

public interface IMessageBrokerProducer
{
    Task ProduceAsync<T>(T message, CancellationToken cancellationToken) where T : class, IEventMessage;
}