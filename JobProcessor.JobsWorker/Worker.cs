using JobProcessor.Domain.Entities.Jobs;
using JobProcessor.Domain.Enums;
using JobProcessor.Domain.Interfaces.Jobs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace JobProcessor.JobsWorker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnectionFactory _factory;
        private readonly ILogger<Worker> _logger;
        private readonly int _maxRetries;
        private readonly int _retryDelayMs;
        private IConnection? _connection;
        private IChannel? _channel;

        public Worker(IServiceScopeFactory scopeFactory, IConnectionFactory factory, IConfiguration configuration, ILogger<Worker> logger)
        {
            _scopeFactory = scopeFactory;
            _factory = factory;
            _logger = logger;
            _maxRetries = configuration.GetValue<int>("Worker:MaxRetries", 5);
            _retryDelayMs = configuration.GetValue<int>("Worker:RetryDelayMs", 5000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConnectToRabbitMQAsync(stoppingToken);

            await _channel!.QueueDeclareAsync(
                queue: "jobs_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            _logger.LogInformation("Fila 'jobs_queue' declarada");

            await _channel.BasicQosAsync(0, 1, false); 
            _logger.LogInformation("QoS configurado para prefetchCount=1");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            _logger.LogInformation("Consumidor criado");

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Mensagem recebida: {Message}", message);

                using var scope = _scopeFactory.CreateScope();
                var jobsRepository = scope.ServiceProvider.GetRequiredService<IJobsRepository>();

                var job = JsonSerializer.Deserialize<Job>(message);
                if (job == null)
                {
                    _logger.LogWarning("Falha ao desserializar o job: {Message}", message);
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    return;
                }

                await jobsRepository.UpdateStatusAsync(job.Id, JobStatusEnum.EmProcessamento, job.RetryCount);
                _logger.LogInformation("Job {JobId} do tipo {JobType} em processamento", job.Id, job.Type);

                try
                {
                    // Simula processamento
                    await Task.Delay(3000, stoppingToken);
                    await jobsRepository.UpdateStatusAsync(job.Id, JobStatusEnum.Concluido, job.RetryCount);
                    _logger.LogInformation("Job {JobId} concluído com sucesso", job.Id);
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar job {JobId}", job.Id);
                    if (job.RetryCount >= 3)
                    {
                        await jobsRepository.UpdateStatusAsync(job.Id, JobStatusEnum.Erro, job.RetryCount);
                        _logger.LogWarning("Job {JobId} falhou após {RetryCount} tentativas", job.Id, job.RetryCount);
                        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        job.RetryCount++;
                        await jobsRepository.UpdateStatusAsync(job.Id, JobStatusEnum.Pendente, job.RetryCount);
                        _logger.LogInformation("Job {JobId} será retentado (tentativa {RetryCount})", job.Id, job.RetryCount);
                        await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "jobs_queue",
                autoAck: false,
                consumer: consumer
            );
            _logger.LogInformation("Consumo iniciado na fila jobs_queue");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ConnectToRabbitMQAsync(CancellationToken stoppingToken)
        {
            int retryCount = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Tentando conectar ao RabbitMQ (tentativa {RetryCount})...", retryCount + 1);
                    _connection = await _factory.CreateConnectionAsync();
                    _logger.LogInformation("Conectado ao RabbitMQ com sucesso!");
                    _channel = await _connection.CreateChannelAsync();
                    _logger.LogInformation("Canal criado com sucesso!");
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogError(ex, "Falha na conexão com RabbitMQ na tentativa {RetryCount}", retryCount);
                    if (retryCount >= _maxRetries)
                    {
                        _logger.LogWarning("Máximo de tentativas ({MaxRetries}) alcançado. Aguardando antes de tentar novamente...", _maxRetries);
                        retryCount = 0;
                    }
                    await Task.Delay(_retryDelayMs, stoppingToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Encerrando o worker...");
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
                _logger.LogInformation("Canal fechado e liberado");
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
                _logger.LogInformation("Conexão com RabbitMQ fechada e liberada");
            }

            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Worker encerrado");
        }
    }
}