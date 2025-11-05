using Microsoft.AspNetCore.Mvc;
using PokedexApi.Dtos;
using PokedexApi.Exceptions;
using PokedexApi.Models;
using PokedexApi.Services;

namespace PokedexApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TrainersController : ControllerBase
{
    private readonly ITrainerService _trainerService;

    public TrainersController(ITrainerService trainerService)
    {
        _trainerService = trainerService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrainerResponseDto>> GetTrainerById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var trainer = await _trainerService.GetByIdAsync(id, cancellationToken);
            return Ok(ToDto(trainer));
        }
        catch (TrainerNotFoundException)
        {
            return NotFound();
        }
    }

    //localhost:PORT/api/v1/trainers?name=ALGUN-NOMBRE
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainerResponseDto>>> GetTrainers([FromQuery] string name, CancellationToken cancellationToken)
    {
        var trainers = await _trainerService.GetAllByNameAsync(name, cancellationToken);
        return Ok(ToDto(trainers));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTrainers([FromBody] List<CreateTrainerRequestDto> request, CancellationToken cancellationToken)
    {
        var trainers = ToModel(request);
        var (successCount, createdTrainers) = await _trainerService.CreateTrainersAsync(trainers, cancellationToken);
        return Ok(new {SuccessCount = successCount, Trainers = ToDto(createdTrainers)});//201 - Individual 
    }

    private static IEnumerable<Trainer> ToModel(List<CreateTrainerRequestDto> trainers)
    {
        return trainers.Select(s => new Trainer
        {
            Name = s.Name,
            Age = s.Age,
            BirthDate = s.BirthDate,
            Medals = s.Medals.Select(m => new Medal {
                Region = m.Region,
                MedalType = Enum.Parse<MedalType>(m.Type)
            }).ToList()
        }).ToList();
    }

    private static IEnumerable<TrainerResponseDto> ToDto(IEnumerable<Trainer> trainers)
    {
        return trainers.Select(ToDto);
    }

    private static TrainerResponseDto ToDto(Trainer trainer)
    {
        return new TrainerResponseDto
        {
            Id = trainer.Id,
            Age = trainer.Age,
            Name = trainer.Name,
            BirthDate = trainer.BirthDate,
            CreatedAt = trainer.CreatedAt,
            Medals = trainer.Medals.Select(ToDto).ToList(),
        };
    }

    private static MedalDto ToDto(Medal medal)
    {
        return new MedalDto
        {
            Region = medal.Region,
            Type = medal.MedalType.ToString()
        };
    }
}