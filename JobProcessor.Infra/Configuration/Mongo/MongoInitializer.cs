using JobProcessor.Infra.Configuration.Mongo.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace JobProcessor.Infra.Configuration.Mongo;

public static class MongoInitializer
{
    public static void InitializeCollections(IMongoDatabase database, IServiceProvider serviceProvider)
    {
        var initializers = serviceProvider.GetServices<IMongoCollectionInitializer>();
        foreach (var initializer in initializers)
        {
            initializer.Initialize(database);
        }
    }
}
