using JobProcessor.Application.Commands.Jobs;
using FluentValidation;

namespace JobProcessor.Application.Validators.Jobs;

public class PostJobValidator : AbstractValidator<PostJobCommand>
{
    public PostJobValidator()
    {
        RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Payload é obrigatório.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de Job inválido.");
    }
}