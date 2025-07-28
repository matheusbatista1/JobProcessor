using JobProcessor.Domain.Entities.Jobs;
using JobProcessor.Domain.Enums;
using JobProcessor.Domain.Interfaces.Jobs;
using MongoDB.Driver;

namespace JobProcessor.Infra.Repositories.Jobs;

public class JobsRepository : IJobsRepository
{
    private readonly IMongoCollection<Job> _collection;

    public JobsRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Job>("Jobs");
    }

    public async Task CreateAsync(Job job)
    {
        await _collection.InsertOneAsync(job);
    }

    public async Task<Job> GetByIdAsync(Guid id)
    {
        return await _collection.Find(j => j.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateStatusAsync(Guid id, JobStatusEnum status, int retryCount)
    {
        var update = Builders<Job>.Update
            .Set(j => j.Status, status)
            .Set(j => j.RetryCount, retryCount);

        await _collection.UpdateOneAsync(j => j.Id == id, update);
    }
}