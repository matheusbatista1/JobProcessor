using MongoDB.Driver;

namespace JobProcessor.Infra.Configuration.Mongo.Interfaces;

public interface IMongoCollectionInitializer
{
    void Initialize(IMongoDatabase database);
}