using TrainerApi.Models;

namespace TrainerApi.Repositories;

public interface ITrainerRepository
{
    Task<Trainer?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<Trainer> CreateAsync(Trainer trainer, CancellationToken cancellationToken);
    Task<IEnumerable<Trainer>> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
    Task UpdateAsync(Trainer trainer, CancellationToken cancellationToken);
}
