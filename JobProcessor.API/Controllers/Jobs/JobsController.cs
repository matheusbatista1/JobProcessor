using JobProcessor.Application.Commands.Jobs;
using JobProcessor.Application.Jobs.Queries.GetJobById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace JobProcessor.API.Controllers.Jobs;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region GET

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetJobByIdQuery(id);
        var job = await _mediator.Send(query);

        if (job == null)
            return NotFound("Job não encontrado.");

        return Ok(job);
    }

    #endregion

    #region POST

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostJobCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    #endregion
}