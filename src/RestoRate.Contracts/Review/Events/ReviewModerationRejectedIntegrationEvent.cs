namespace RestoRate.Contracts.Review.Events;

public record ReviewModerationRejectedIntegrationEvent(
    Guid ReviewId,
    string Reason
);
