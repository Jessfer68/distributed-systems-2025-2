using PokemonApi.Dtos;
using PokemonApi.Infrastructure.Entities;
using PokemonApi.Models;

namespace PokemonApi.Mappers;

public static class PokemonMapper
{
    //Extension method
    public static Pokemon ToModel(this PokemonEntity pokemonEntity)
    {
        if (pokemonEntity is null)
        {
            return null;
        }

        return new Pokemon
        {
            Id = pokemonEntity.Id,
            Name = pokemonEntity.Name,
            Type = pokemonEntity.Type,
            Level = pokemonEntity.Level,
            Stats = new Stats
            {
                Attack = pokemonEntity.Attack,
                Speed = pokemonEntity.Speed,
                Defense = pokemonEntity.Defense
            }
        };
    }

    public static PokemonEntity ToEntity(this Pokemon pokemon)
    {
        return new PokemonEntity
        {
            Id = pokemon.Id,
            Level = pokemon.Level,
            Type = pokemon.Type,
            Name = pokemon.Name,
            Attack = pokemon.Stats.Attack,
            Defense = pokemon.Stats.Defense,
            Speed = pokemon.Stats.Speed
        };
    }

    public static Pokemon ToModel(this CreatePokemonDto requestPokemonDto)
    {
        return new Pokemon
        {
            Name = requestPokemonDto.Name,
            Type = requestPokemonDto.Type,
            Level = requestPokemonDto.Level,
            Stats = new Stats
            {
                Attack = requestPokemonDto.Stats.Attack,
                Speed = requestPokemonDto.Stats.Speed,
                Defense = requestPokemonDto.Stats.Defense
            }
        };
    }

    public static PokemonResponseDto ToResponseDto(this Pokemon pokemon)
    {
        return new PokemonResponseDto
        {
            Id = pokemon.Id,
            Name = pokemon.Name,
            Type = pokemon.Type,
            Level = pokemon.Level,
            Stats = new StatsDto
            {
                Attack = pokemon.Stats.Attack,
                Speed = pokemon.Stats.Speed,
                Defense = pokemon.Stats.Defense
            }
        };
    }
}
