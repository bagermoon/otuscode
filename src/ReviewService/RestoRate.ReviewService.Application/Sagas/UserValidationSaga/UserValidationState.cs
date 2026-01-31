using MassTransit;

namespace RestoRate.ReviewService.Application.Sagas.UserValidationSaga;

public sealed class UserValidationState :
    SagaStateMachineInstance,
    ISagaVersion
{
    public int Version { get; set; }
    public Guid CorrelationId { get; set; } // This is the UserId
    public string CurrentState { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public bool? IsValid { get; set; }
}