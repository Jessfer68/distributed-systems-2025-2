using Google.Protobuf.Collections;
using Grpc.Core;
using PokedexApi.Exceptions;
using PokedexApi.Infrastructure.Grpc;
using PokedexApi.Models;
using Medal = PokedexApi.Models.Medal;
using MedalType = PokedexApi.Models.MedalType;

namespace PokedexApi.Gateways;

public class TrainerGateway : ITrainerGateway
{
    private readonly TrainerService.TrainerServiceClient _client;

    public TrainerGateway(TrainerService.TrainerServiceClient client)
    {
        _client = client;   
    }

    public async Task<Trainer> GetTrainerById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var trainer = await _client.GetTrainerByIdAsync(new TrainerByIdRequest { Id = id}, 
                cancellationToken: cancellationToken);
            return ToModel(trainer);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new TrainerNotFoundException(id);
        }
    }

    public async Task<(int, IList<Trainer>)> CreateTrainersAsync(IEnumerable<Trainer> trainers,
        CancellationToken cancellationToken)
    {
        using var call = _client.CreateTrainers(cancellationToken: cancellationToken);
        foreach (var trainer in trainers)
        {
            var request = new CreateTrainerRequest
            {
                Name = trainer.Name,
                Age = trainer.Age,
                Birthdate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(trainer.BirthDate),
                Medals = { trainer.Medals.Select(m => new Infrastructure.Grpc.Medal
                {
                    Region = m.Region,
                    Type = (Infrastructure.Grpc.MedalType) m.MedalType,
                }) }
            };

            await call.RequestStream.WriteAsync(request, cancellationToken);
            await Task.Delay(1000, cancellationToken); //1 sec
        }

        await call.RequestStream.CompleteAsync();
        var response = await call;
        return (response.SuccessCount, ToModel(response.Trainers));
    }

    public async IAsyncEnumerable<Trainer> GetTrainersByName(string name, CancellationToken cancellationToken)
    {
        var request = new TrainersByNameRequest { Name = name };
        using var call = _client.GetAllTrainersByName(request, cancellationToken: cancellationToken);
        while (await call.ResponseStream.MoveNext(cancellationToken))
        {
            yield return ToModel(call.ResponseStream.Current);
        }
    }

    private static IList<Trainer> ToModel(RepeatedField<TrainerResponse> trainerResponse)
    {
        return trainerResponse.Select(ToModel).ToList();
    }

    private static Trainer ToModel(TrainerResponse trainerResponse)
    {
        return new Trainer
        {
            Id = trainerResponse.Id,
            Name = trainerResponse.Name,
            Age = trainerResponse.Age,
            BirthDate = trainerResponse.Birthdate.ToDateTime(),
            CreatedAt = trainerResponse.CreatedAt.ToDateTime(),
            Medals = trainerResponse.Medals.Select(ToModel).ToList()
        };
    }

    private static Medal ToModel(Infrastructure.Grpc.Medal medal)
    {
        return new Medal
        {
            Region = medal.Region,
            MedalType = (MedalType)(int) medal.Type,
        };
    }
}