using JobProcessor.Domain.Entities.Jobs;
using JobProcessor.Domain.Enums;

namespace JobProcessor.Domain.Interfaces.Jobs;

public interface IJobsRepository
{
    Task CreateAsync(Job job);
    Task<Job> GetByIdAsync(Guid id);
    Task UpdateStatusAsync(Guid id, JobStatusEnum status, int retryCount);
}