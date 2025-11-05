using Google.Protobuf.WellKnownTypes;
using TrainerApi.Infrastructure.Documents;
using TrainerApi.Models;

namespace TrainerApi.Mappers;

public static class TrainerMapper 
{
    public static Trainer ToDomain(this TrainerDocument document) 
    {
        if (document == null)
        {
            return null;
        }

        return new Trainer 
        {
            Id = document.Id,
            Name = document.Name,
            Age = document.Age,
            Birthdate = document.Birthdate,
            CreatedAt = document.CreatedAt,
            Medals = document.Medals.Select(m => m.ToDomain()).ToList()
        };
    }

    public static Models.Medal ToDomain(this MedalDocument document) 
    {
        return new Models.Medal 
        {
            Region = document.Region,
            Type = (Models.MedalType)(int)document.Type
        };
    }

    public static TrainerResponse ToResponse(this Trainer trainer) 
    {
        return new TrainerResponse 
        {
            Id = trainer.Id,
            Name = trainer.Name,
            Age = trainer.Age,
            Birthdate = Timestamp.FromDateTime(trainer.Birthdate.ToUniversalTime()),
            CreatedAt = Timestamp.FromDateTime(trainer.CreatedAt.ToUniversalTime()),
            Medals = { trainer.Medals.Select(m => m.ToResponse()) }
        };
    }

    public static Trainer ToModel(this CreateTrainerRequest request) 
    {
        return new Trainer 
        {
            Name = request.Name,
            Age = request.Age,
            Birthdate = request.Birthdate.ToDateTime(),
            Medals = request.Medals.Select(m => m.ToModel()).ToList()
        };
    }
    
    public static TrainerDocument ToDocument(this Trainer trainer) 
    {
        return new TrainerDocument 
        {
            Id = trainer.Id,
            Name = trainer.Name,
            Age = trainer.Age,
            Birthdate = trainer.Birthdate,
            CreatedAt = trainer.CreatedAt,
            Medals = trainer.Medals.Select(m => m.ToDocument()).ToList()
        };
    }

    public static MedalDocument ToDocument(this Models.Medal medal) 
    {
        return new MedalDocument 
        {
            Region = medal.Region,
            Type = (MedalTypeMongo) medal.Type
        };
    }

    public static Models.Medal ToModel(this Medal request) 
    {
        return new Models.Medal 
        {
            Region = request.Region,
            Type = (Models.MedalType)(int)request.Type
        };
    }

    private static Medal ToResponse(this Models.Medal medal) 
    {
        return new Medal 
        {
            Region = medal.Region,
            Type = (MedalType)(int)medal.Type
        };
    }
}
