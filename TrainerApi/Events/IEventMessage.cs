namespace TrainerApi.Events;

public interface IEventMessage
{
    string Topic { get; }
    string? GetEventKey() => null;
}