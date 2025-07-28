using JobProcessor.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobProcessor.Domain.Entities.Jobs;

public class Job
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public JobsTypeEnum Type { get; set; }

    public string Payload { get; set; }

    public JobStatusEnum Status { get; set; } = JobStatusEnum.Pendente;

    public int RetryCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}