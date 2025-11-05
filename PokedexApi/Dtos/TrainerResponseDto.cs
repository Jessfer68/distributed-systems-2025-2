namespace PokedexApi.Dtos;

public class TrainerResponseDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<MedalDto> Medals { get; set; }
}

public class MedalDto
{
    public string Region { get; set; }
    public string Type { get; set; }
}