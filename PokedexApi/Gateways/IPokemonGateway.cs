using PokedexApi.Models;

namespace PokedexApi.Gateways;

//Como si fuera un repositorio
//Clean architecture y a Hexagonal Architecture
public interface IPokemonGateway
{
    Task<Pokemon> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken);
    
    Task<IList<Pokemon>> GetPokemonsByNameAsync(string name, CancellationToken cancellationToken);
    
    Task<Pokemon> CreatePokemonAsync(Pokemon pokemon, CancellationToken cancellationToken);
}
