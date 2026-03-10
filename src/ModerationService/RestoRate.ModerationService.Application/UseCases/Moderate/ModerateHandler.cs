using Ardalis.Result;

using Mediator;

using RestoRate.ModerationService.Domain.Abstractions;

namespace RestoRate.ModerationService.Application.UseCases.Moderate;

public sealed class ModerateHandler(
    ITextModerator textModerator)
    : ICommandHandler<ModerateCommand, Result>
{
    public ValueTask<Result> Handle(
        ModerateCommand request,
        CancellationToken cancellationToken)
        => ValueTask.FromResult(textModerator.Moderate(request.Comment));
}
