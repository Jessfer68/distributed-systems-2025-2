using System.Text.Json.Serialization;

namespace TrainerApi.Events;

public class TrainerCreatedEvent : IEventMessage
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MedalEvent> Medals { get; set; } = new();

    [JsonIgnore]
    public string Topic => "trainer-created";
    
    public string? GetEventKey() => Id;
}