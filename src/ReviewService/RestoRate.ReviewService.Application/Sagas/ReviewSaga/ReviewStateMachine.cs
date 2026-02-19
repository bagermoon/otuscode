using MassTransit;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.UseCases.Reviews.Approve;
using RestoRate.ReviewService.Application.UseCases.Reviews.MoveToModerationPending;
using RestoRate.ReviewService.Application.UseCases.Reviews.Reject;

using Mediator;

using Microsoft.Extensions.DependencyInjection;
using RestoRate.Contracts.Moderation.Events;

namespace RestoRate.ReviewService.Application.Sagas.ReviewSaga;

public class ReviewStateMachine : MassTransitStateMachine<ReviewState>
{
    public State Validating { get; private set; } = default!;

    public State ValidationOk { get; private set; } = default!;
    public State ValidationFailed { get; private set; } = default!;

    public State ModerationApproved { get; private set; } = default!;
    public State ModerationRejected { get; private set; } = default!;

    public Event<ReviewValidationRequested> ReviewValidationRequested { get; private set; } = default!;
    public Event<RestaurantValidationCompleted> RestaurantValidationCompleted { get; private set; } = default!;
    public Event<UserValidationCompleted> UserValidationCompleted { get; private set; } = default!;
    public Event<ReviewModeratedEvent> ReviewModeratedEvent { get; private set; } = default!;

    public ReviewStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => ReviewValidationRequested, x =>
        {
            x.CorrelateById(context => context.Message.ReviewId);
            x.SelectId(context => context.Message.ReviewId);
        });

        Event(() => RestaurantValidationCompleted, x =>
        {
            x.CorrelateById(context => context.Message.ReviewId);
        });

        Event(() => UserValidationCompleted, x =>
        {
            x.CorrelateById(context => context.Message.ReviewId);
        });

        Event(() => ReviewModeratedEvent, x =>
        {
            x.CorrelateById(context => context.Message.ReviewId);
        });

        Initially(
            When(ReviewValidationRequested)
                .Then(context =>
                {
                    context.Saga.RestaurantId = context.Message.RestaurantId;
                    context.Saga.UserId = context.Message.UserId;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Validating));

        During(Validating,
            Ignore(ReviewValidationRequested),
            When(RestaurantValidationCompleted)
                .Then(context =>
                {
                    context.Saga.RestaurantValidatedAt = context.Message.ValidatedAt;
                    context.Saga.IsRestaurantValid = context.Message.IsValid;
                })
                .If(context => context.Saga.IsUserValid.HasValue,
                    DecideValidation),
            When(UserValidationCompleted)
                .Then(context =>
                {
                    context.Saga.UserValidatedAt = context.Message.ValidatedAt;
                    context.Saga.IsUserValid = context.Message.IsValid;
                })
                .If(context => context.Saga.IsRestaurantValid.HasValue,
                    DecideValidation)
        );

        During(ValidationOk,
            Ignore(ReviewValidationRequested),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            When(ReviewModeratedEvent)
                .IfElse(
                    context => context.Message.Approved,
                    thenBinder => thenBinder
                        .ThenAsync(ApproveReviewAsync)
                        .TransitionTo(ModerationApproved)
                        .Finalize(),
                    elseBinder => elseBinder
                        .ThenAsync(RejectReviewAsync)
                        .TransitionTo(ModerationRejected)
                        .Finalize())
        );

        During(ValidationFailed,
            Ignore(ReviewValidationRequested),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            Ignore(ReviewModeratedEvent));

        During(ModerationApproved,
            Ignore(ReviewValidationRequested),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            Ignore(ReviewModeratedEvent));

        During(ModerationRejected,
            Ignore(ReviewValidationRequested),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            Ignore(ReviewModeratedEvent));

        SetCompletedWhenFinalized();
    }

    private EventActivityBinder<ReviewState, TMessage> DecideValidation<TMessage>(EventActivityBinder<ReviewState, TMessage> binder)
        where TMessage : class
        => binder.IfElse(
            context => context.Saga.IsRestaurantValid == true && context.Saga.IsUserValid == true,
            thenBinder => thenBinder
                .ThenAsync(MoveReviewToModerationPendingAsync)
                .TransitionTo(ValidationOk),
            elseBinder => elseBinder
                .ThenAsync(RejectReviewAsync)
                .TransitionTo(ValidationFailed))
                .Finalize();

    private static async Task MoveReviewToModerationPendingAsync<TMessage>(BehaviorContext<ReviewState, TMessage> context)
        where TMessage : class
    {
        var provider = context.GetPayload<IServiceProvider>();
        var sender = provider.GetRequiredService<ISender>();

        await sender.Send(
            new MoveReviewToModerationPendingCommand(context.Saga.CorrelationId),
            context.CancellationToken);
    }
    private static async Task RejectReviewAsync<TMessage>(BehaviorContext<ReviewState, TMessage> context)
        where TMessage : class
    {
        var provider = context.GetPayload<IServiceProvider>();
        var sender = provider.GetRequiredService<ISender>();

        await sender.Send(
            new RejectReviewCommand(context.Saga.CorrelationId),
            context.CancellationToken);
    }
    private static async Task ApproveReviewAsync<TMessage>(BehaviorContext<ReviewState, TMessage> context)
        where TMessage : class
    {
        var provider = context.GetPayload<IServiceProvider>();
        var sender = provider.GetRequiredService<ISender>();

        await sender.Send(
            new ApproveReviewCommand(context.Saga.CorrelationId),
            context.CancellationToken);
    }
}
