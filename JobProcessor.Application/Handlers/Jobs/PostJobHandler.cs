using JobProcessor.Domain.Entities.Jobs;
using JobProcessor.Domain.Interfaces.Jobs;
using JobProcessor.Domain.Interfaces.Services.RabbitMQ;
using MediatR;
using System.Text.Json;

namespace JobProcessor.Application.Commands.Jobs
{
    public class PostJobHandler : IRequestHandler<PostJobCommand, Job>
    {
        private readonly IJobsRepository _jobsRepository;
        private readonly IRabbitMqService _rabbitMqService;

        public PostJobHandler(IJobsRepository jobsRepository, IRabbitMqService rabbitMqService)
        {
            _jobsRepository = jobsRepository;
            _rabbitMqService = rabbitMqService;
        }

        public async Task<Job> Handle(PostJobCommand request, CancellationToken cancellationToken)
        {
            var job = new Job
            {
                Id = Guid.NewGuid(),
                Payload = request.Payload,
                Type = request.Type
            };

            await _jobsRepository.CreateAsync(job);

            var message = JsonSerializer.Serialize(job);
            await _rabbitMqService.PublishAsync("jobs_queue", message);

            return job;
        }
    }
}