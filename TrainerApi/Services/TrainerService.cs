using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using TrainerApi.Events;
using TrainerApi.Infrastructure.Producers;
using TrainerApi.Repositories;
using TrainerApi.Mappers;
using TrainerApi.Models;

namespace TrainerApi.Services;

public class TrainerService : TrainerApi.TrainerService.TrainerServiceBase
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly IMessageBrokerProducer _producer;
    private static int LegalMexicanAge = 18;
    
    public TrainerService(ITrainerRepository trainerRepository, IMessageBrokerProducer producer) 
    {
        _trainerRepository = trainerRepository;
        _producer = producer;
    }

    public override async Task<TrainerResponse> GetTrainerById(TrainerByIdRequest request, ServerCallContext context)
    {
        var trainer = await GetTrainerAsync(request.Id, context.CancellationToken);
        return trainer.ToResponse();
    }

    public override async Task<Empty> UpdateTrainer(UpdateTrainerRequest request, ServerCallContext context)
    {
        if(!IdFormatIsValid(request.Id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid trainer ID")); //400
        if(request.Age < LegalMexicanAge)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid trainer age"));
        if(string.IsNullOrEmpty(request.Name) || request.Name.Length < 3)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid trainer name"));
        
        var trainer = await GetTrainerAsync(request.Id, context.CancellationToken);
        trainer.Name = request.Name;
        trainer.Age = request.Age;
        trainer.Birthdate = request.Birthdate.ToDateTime();
        trainer.Medals = request.Medals.Select(s => s.ToModel()).ToList();
        
        var trainers = await _trainerRepository.GetByNameAsync(request.Name, context.CancellationToken);
        var trainerAlreadyExists = trainers.Any(t => t.Id != trainer.Id);
        if(trainerAlreadyExists)
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Trainer already exists"));

        await _trainerRepository.UpdateAsync(trainer, context.CancellationToken);
        
        return new Empty();
    }

    public override async Task<Empty> DeleteTrainer(TrainerByIdRequest request, ServerCallContext context)
    {
        if(!IdFormatIsValid(request.Id)) 
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid trainer ID")); //400
        
        var trainer = await GetTrainerAsync(request.Id, context.CancellationToken);
        await _trainerRepository.DeleteAsync(trainer.Id, context.CancellationToken);
        return new Empty();
    }

    public override async Task<CreateTrainerResponse> CreateTrainers(IAsyncStreamReader<CreateTrainerRequest> requestStream, ServerCallContext context)
    {
        var createdTrainers = new List<TrainerResponse>();

        while(await requestStream.MoveNext(context.CancellationToken))
        {
            var request = requestStream.Current;
            var trainer = request.ToModel();
            var trainerExists = await _trainerRepository.GetByNameAsync(trainer.Name, context.CancellationToken);
            if (trainerExists.Any())
                continue;
            var createdTrainer = await _trainerRepository.CreateAsync(trainer, context.CancellationToken);
            createdTrainers.Add(createdTrainer.ToResponse());

            var ev = new TrainerCreatedEvent
            {
                Id = createdTrainer.Id,
                Name = createdTrainer.Name,
                Age = createdTrainer.Age,
                BirthDate =  createdTrainer.Birthdate,
                CreatedAt = createdTrainer.CreatedAt,
                Medals = createdTrainer.Medals.Select(s => new MedalEvent
                {
                    Region = s.Region,
                    Type = s.Type.ToString()
                }).ToList()
            };

            await _producer.ProduceAsync(ev, context.CancellationToken);
        }

        return new CreateTrainerResponse 
        {
            SuccessCount = createdTrainers.Count,
            Trainers = {createdTrainers},
        };
    }

    public override async Task GetAllTrainersByName(TrainersByNameRequest request, IServerStreamWriter<TrainerResponse> 
            responseStream, ServerCallContext context) 
    {
        var trainers = await _trainerRepository.GetByNameAsync(request.Name, context.CancellationToken);

        foreach (var trainer in trainers) 
        {
            if (context.CancellationToken.IsCancellationRequested) 
                break;

            await responseStream.WriteAsync(trainer.ToResponse());
            await Task.Delay(5000, context.CancellationToken);
        }
    }

    private async Task<Trainer> GetTrainerAsync(string id, CancellationToken cancellationToken)
    {
        var trainer = await _trainerRepository.GetByIdAsync(id, cancellationToken);
        return trainer ?? throw new RpcException(new Status(StatusCode.NotFound, $"Trainer with ID {id} not found."));
    } 
    
    private static bool IdFormatIsValid(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && id.Length > 20;
    }
}
