using System;

using MassTransit;

namespace RestoRate.ReviewService.Application.Sagas.ReviewSaga;

public class ReviewState : SagaStateMachineInstance,
    ISagaVersion
{
    public int Version { get; set; }
    public string CurrentState { get; set; } = default!;
    public Guid CorrelationId { get; set; } // This is the ReviewId
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool? IsRestaurantValid { get; set; }
    public DateTime? RestaurantValidatedAt { get; set; }

    public bool? IsUserValid { get; set; }
    public DateTime? UserValidatedAt { get; set; }

    // Used by MassTransit CompositeEvent to track completion of multiple events.
    public int ValidationCompleted { get; set; }
}