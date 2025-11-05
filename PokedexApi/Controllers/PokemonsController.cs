using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PokedexApi.Dtos;
using PokedexApi.Exceptions;
using PokedexApi.Mappers;
using PokedexApi.Services;

namespace PokedexApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class PokemonsController : ControllerBase
{
    private readonly IPokemonService _pokemonService;
    private readonly ILogger<PokemonsController> _logger;

    public PokemonsController(IPokemonService pokemonService, ILogger<PokemonsController>  logger)
    {
        _pokemonService = pokemonService;
        _logger = logger;
    }

    //localhost:PORT/api/v1/pokemons/ID
    // HTTP STATUS 
    // 200 - OK (Si exista el pokemon)
    // 400 - BadRequest (Si el formato del id es incorrecto) --- CASI NO SE USA
    // 404 - NotFound (No existe el pokemon)
    // 500 - Internal Server Error
    // HTTP Verb - GET
    [HttpGet("{id}", Name = "GetPokemonByIdAsync")]
    [Authorize(Policy = "Read")]
    public async Task<ActionResult<PokemonResponse>> GetPokemonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pokemon = await _pokemonService.GetPokemonByIdAsync(id, cancellationToken);
        return pokemon is null ? NotFound() : Ok(pokemon.ToResponse());
    }

    //localhost:PORT/api/v1/pokemons?name=pikachu&type=fire
    //HTTP STATUS - GET
    // 200 - OK (Si existe o no pokemon (se regresa listado vacio))
    // 400 - BadRequest (Si alguno de los query parameters son incorrector)
    // 500 - Internal Server Error
    [HttpGet]
    [Authorize(Policy = "Read")]
    public async Task<ActionResult<IList<PokemonResponse>>> GetPokemonsAsync([FromQuery] string name, 
            [FromQuery] string type, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(type)) {
            return BadRequest(new {Message = "Type query parameter is required"});
        }

        var pokemons = await _pokemonService.GetPokemonsAsync(name, type, cancellationToken);
        return Ok(pokemons.ToResponse());
    }

    //localhost:PORT/api/v1/pokemons
    // Body Request - JSON {campo1:...,campo2:....}
    //HTTP Verb - POST
    // HTTP STATUS
    // 400 - BadRequest (Si el usuario manda informacion en el body incorrecta ejemplo: manda un string en lugar de un int)
    // 409 - Conflict (Ya existe otra entidad previamente registrada)
    // 422 - Entidad no procesable (Por alguna regla de negocio internal)
    // 500 - Internal Error
    // 200 - Ok (El recurso creado + id) -- No sigue muy bien las buenas practicas de RESTFUL
    // 201 - Created (El recurso creado + id) -- Response Header (href: hace referencia al get para obtener el recurso)
    //key: localhost:PORT/api/v1/pokemons/AQUI_VA_EL_ID_GENERADO
    // 202 - Accepted (Procesamiento async)
    [HttpPost]
    [Authorize(Policy = "Write")]
    public async Task<ActionResult<PokemonResponse>> CreatePokemonAsync([FromBody] CreatePokemonRequest createPokemon,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidAttack(createPokemon.Stats.Attack))
            {
                //{"message": "Attack does not have a valid value"}
                return BadRequest(new { Message = "Attack does not have a valid value" });
            }

            var pokemon = await _pokemonService.CreatePokemonAsync(createPokemon.ToModel(), cancellationToken);
            
            _logger.LogInformation("Pokemon created: {id}", pokemon.Id);
            //201 - key: localhost:PORT/api/v1/pokemons/AQUI_VA_EL_ID_GENERADO
            return CreatedAtRoute(nameof(GetPokemonByIdAsync), new { id = pokemon.Id }, pokemon.ToResponse());
        }
        catch (PokemonAlreadyExistsException e)
        {
            //409 - Conflicto - {"message": "Pokemon NAME already exists"}
            return Conflict(new  { Message = e.Message });
        }
    }

    //localhost:PORT/api/v1/pokemons/ID
    // HTTP Verb - DELETE
    // HTTP STATUS
    // 204 - No Content (Si se borro correctamente)
    // 200 - Ok (Si se borro correctamente) -- No sigue muy bien las buenas practicas de RESTFUL
    // {"message": "Pokemon deleted successfully"}
    // 404 - NotFound (No existe el pokemon que se quiere borrar)
    // 500 - Internal Server Error
    [HttpDelete("{id}")]
    [Authorize(Policy = "Write")]
    public async Task<ActionResult> DeletePokemonAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _pokemonService.DeletePokemonAsync(id, cancellationToken);
            return NoContent(); //204
        }
        catch(PokemonNotFoundException)
        {
            return NotFound(); //404
        }
    }

    //localhost:PORT/api/v1/Pokemons/ID
    // HTTP Verb - PUT
    // HTTP Status
    // 204 - No Content --- Es mas orientado a RestFul
    // 200 - OK (Retornar la entidad actualizada)
    // 404 - NotFound
    // 400 - Validaciones de los campos sean incorrectos
    // 500 - Internal Server Error
    [HttpPut("{id}")]
    [Authorize(Policy = "Write")]
    public async Task<ActionResult> UpdatePokemonAsync(Guid id, [FromBody] UpdatePokemonRequest pokemon, CancellationToken cancellationToken)
    {
        try
        {
            if(!IsValidAttack(pokemon.Stats.Attack))
            {
                return BadRequest(new {Message = "Invalid Attack Value"}); //400
            }

            await _pokemonService.UpdatePokemonAsync(pokemon.ToModel(id), cancellationToken);
            return NoContent(); //204
        }
        catch(PokemonNotFoundException)
        {
            return NotFound(); //404
        }
        catch(PokemonAlreadyExistsException ex)
        {
            return Conflict(new {Message = ex.Message}); //409
        }
    }

    //localhost:PORT/api/v1/Pokemons/ID
    // HTTP Verb - PATCH
    // 200 - Ok(Retornar la entidad actualizada) -- Mas recomendado
    // 204 - NoContent
    // 404 - NotFound
    // 400 - Validacion
    // 500 - Internal Server Error
    [HttpPatch("{id}")]
    [Authorize(Policy = "Write")]
    public async Task<ActionResult<PokemonResponse>> PatchPokemonAsync(Guid id, [FromBody] PatchPokemonRequest pokemonRequest, CancellationToken cancellationToken)
    {
        try
        {
            if(pokemonRequest.Attack.HasValue && !IsValidAttack(pokemonRequest.Attack.Value))
            {
                return BadRequest(new {Message = "Invalid Attack Value"}); //400
            }

            var pokemon = await _pokemonService.PatchPokemonAsync(id, pokemonRequest.Name, pokemonRequest.Type, pokemonRequest.Attack,
                pokemonRequest.Defense, pokemonRequest.Speed, cancellationToken);

            return Ok(pokemon.ToResponse()); //200
        }
        catch(PokemonNotFoundException)
        {
            return NotFound(); //404
        }
        catch(PokemonAlreadyExistsException ex)
        {
            return Conflict(new {Message = ex.Message}); //409
        }
    }

    private static bool IsValidAttack(int attack)
    {
        return attack > 0;
    }
}
