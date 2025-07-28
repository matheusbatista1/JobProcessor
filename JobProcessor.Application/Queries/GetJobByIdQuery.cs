using JobProcessor.Domain.Entities.Jobs;
using MediatR;

namespace JobProcessor.Application.Jobs.Queries.GetJobById;

public record GetJobByIdQuery(Guid Id) : IRequest<Job>;