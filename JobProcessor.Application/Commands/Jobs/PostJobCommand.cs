using JobProcessor.Domain.Entities.Jobs;
using JobProcessor.Domain.Enums;
using MediatR;

namespace JobProcessor.Application.Commands.Jobs;

public record PostJobCommand(string Payload, JobsTypeEnum Type) : IRequest<Job>;