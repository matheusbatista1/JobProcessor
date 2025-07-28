using JobProcessor.Domain.Entities.Jobs;
using JobProcessor.Infra.Configuration.Mongo.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace JobProcessor.Infra.Configuration.Mongo.Initializers;

public class JobsCollectionInitializer : IMongoCollectionInitializer
{
    public void Initialize(IMongoDatabase database)
    {
        var collectionName = "Jobs";
        var collections = database.ListCollectionNames().ToList();
        if (!collections.Contains(collectionName))
        {
            var options = new CreateCollectionOptions<Job>
            {
                Validator = new BsonDocument
                {
                    {
                        "$jsonSchema", new BsonDocument
                        {
                            { "bsonType", "object" },
                            { "required", new BsonArray { "Type", "Payload", "Status" } },
                            { "properties", new BsonDocument
                                {
                                    { "Type", new BsonDocument { { "bsonType", "string" } } },
                                    { "Payload", new BsonDocument { { "bsonType", "string" } } },
                                    { "Status", new BsonDocument { { "enum", new BsonArray { 0, 1, 2, 3 } } } },
                                }
                            }
                        }
                    }
                }
            };

            database.CreateCollection(collectionName, options);
        }
    }
}
