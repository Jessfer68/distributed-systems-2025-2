using System.Text.Json.Serialization;

namespace TrainerApi.Events;

public class TrainerDeletedEvent : IEventMessage
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime DeletedAt { get; set; }

    [JsonIgnore]
    public string Topic => "trainer-deleted";

    public string GetEventKey() => Id;
}