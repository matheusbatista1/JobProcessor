using JobProcessor.Domain.Interfaces.Jobs;
using JobProcessor.Domain.Interfaces.Services.RabbitMQ;
using JobProcessor.Infra.Configuration.Mongo;
using JobProcessor.Infra.Configuration.Mongo.Initializers;
using JobProcessor.Infra.Configuration.Mongo.Interfaces;
using JobProcessor.Infra.Repositories.Jobs;
using JobProcessor.Infra.Services.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using RabbitMQ.Client;

namespace JobProcessor.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")!;
        var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE")!;
        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
        var rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";

        var mongoClient = MongoDbConnector.ConnectWithRetry(connectionString);
        var database = mongoClient.GetDatabase(databaseName);

        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddSingleton<IMongoDatabase>(database);

        services.AddScoped<IMongoCollectionInitializer, JobsCollectionInitializer>();
        services.AddScoped<IJobsRepository, JobsRepository>();

        services.AddSingleton<IConnectionFactory>(_ =>
            new ConnectionFactory
            {
                HostName = rabbitHost,
                UserName = rabbitUser,
                Password = rabbitPass
            });

        services.AddScoped<IRabbitMqService, RabbitMqService>();

        return services;
    }
}