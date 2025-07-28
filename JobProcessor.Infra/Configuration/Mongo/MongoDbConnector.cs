using MongoDB.Bson;
using MongoDB.Driver;

namespace JobProcessor.Infra.Configuration.Mongo;

public static class MongoDbConnector
{
    public static MongoClient ConnectWithRetry(string connectionString, int maxRetries = 5, int delayMs = 3000)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var client = new MongoClient(connectionString);

                var db = client.GetDatabase("admin");
                db.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();

                Console.WriteLine("Conectado ao MongoDB com sucesso!");
                return client;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tentativa {attempt} de conexão ao Mongo falhou: {ex.Message}");

                if (attempt == maxRetries)
                    throw;

                Thread.Sleep(delayMs);
            }
        }

        throw new Exception("Falha ao conectar no MongoDB após várias tentativas.");
    }
}