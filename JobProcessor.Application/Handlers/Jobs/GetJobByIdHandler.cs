using JobProcessor.Domain.Entities.Jobs;
using JobProcessor.Domain.Interfaces.Jobs;
using MediatR;

namespace JobProcessor.Application.Jobs.Queries.GetJobById;

public class GetJobByIdHandler : IRequestHandler<GetJobByIdQuery, Job>
{
    private readonly IJobsRepository _repository;

    public GetJobByIdHandler(IJobsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Job> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id);
    }
}