namespace RestoRate.ModerationService.Application.Interfaces;

public interface ITextModerator
{
    (bool IsApproved, string? RejectReason) Moderate(string? text);
}
