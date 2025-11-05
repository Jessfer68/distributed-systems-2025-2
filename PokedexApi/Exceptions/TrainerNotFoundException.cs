namespace PokedexApi.Exceptions;

public class TrainerNotFoundException : Exception
{
    public TrainerNotFoundException(string id) : base($"Trainer not found {id}")
    {
        
    }
}