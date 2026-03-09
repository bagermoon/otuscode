using Ardalis.Result;

using Mediator;

namespace RestoRate.ModerationService.Application.UseCases.Moderate;

public sealed record ModerateCommand(
    Guid ReviewId,
    string? Comment)
    : ICommand<Result>;
