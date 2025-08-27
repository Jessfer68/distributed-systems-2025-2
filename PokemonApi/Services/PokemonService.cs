using System.ServiceModel;
using PokemonApi.Dtos;
using PokemonApi.Repositories;
using PokemonApi.Mappers;
using PokemonApi.Validators;

namespace PokemonApi.Services;

public class PokemonService : IPokemonService
{
    private readonly IPokemonRepository _pokemonRepository;

    public PokemonService(IPokemonRepository pokemonRepository)
    {
        _pokemonRepository = pokemonRepository;
    }

    public async Task<PokemonResponseDto> CreatePokemon(CreatePokemonDto pokemonRequest, CancellationToken cancellationToken)
    {
        //Fluent methods
        pokemonRequest.ValidateName().ValidateLevel().ValidateType();

        if (await PokemonAlreadyExists(pokemonRequest.Name, cancellationToken))
        {
            throw new FaultException("Pokemon already exists");
        }

        var pokemon = await _pokemonRepository.CreateAsync(pokemonRequest.ToModel(), cancellationToken);

        return pokemon.ToResponseDto();
    }

    private async Task<bool> PokemonAlreadyExists(string name, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonRepository.GetByNameAsync(name, cancellationToken);
        return pokemon is not null;
    }
}