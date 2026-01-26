using MassTransit;

using RestoRate.ReviewService.Application.Sagas.Messages;
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
    public Event ValidationsCompleted { get; private set; } = default!;

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

        CompositeEvent(
            () => ValidationsCompleted,
            x => x.ValidationCompleted,
            RestaurantValidationCompleted,
            UserValidationCompleted);

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
                }),
            When(UserValidationCompleted)
                .Then(context =>
                {
                    context.Saga.UserValidatedAt = context.Message.ValidatedAt;
                    context.Saga.IsUserValid = context.Message.IsValid;
                }),
            When(ValidationsCompleted)
                .IfElse(
                    context => context.Saga.IsRestaurantValid == true && context.Saga.IsUserValid == true,
                    thenBinder => thenBinder
                        .ThenAsync(async context =>
                        {
                            var provider = context.GetPayload<IServiceProvider>();
                            var sender = provider.GetRequiredService<ISender>();

                            await sender.Send(
                                new MoveReviewToModerationPendingCommand(context.Saga.CorrelationId),
                                context.CancellationToken);
                        })
                        .TransitionTo(ValidationOk),
                    elseBinder => elseBinder
                        .ThenAsync(async context =>
                        {
                            var provider = context.GetPayload<IServiceProvider>();
                            var sender = provider.GetRequiredService<ISender>();

                            await sender.Send(
                                new RejectReviewCommand(context.Saga.CorrelationId),
                                context.CancellationToken);
                        })
                        .TransitionTo(ValidationFailed))
        );

        During(ValidationOk,
            Ignore(ReviewValidationRequested),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            Ignore(ValidationsCompleted),
            When(ReviewModeratedEvent)
                .IfElse(
                    context => context.Message.Approved,
                    thenBinder => thenBinder
                        .ThenAsync(async context =>
                        {
                            var provider = context.GetPayload<IServiceProvider>();
                            var sender = provider.GetRequiredService<ISender>();

                            await sender.Send(
                                new ApproveReviewCommand(context.Saga.CorrelationId),
                                context.CancellationToken);
                        })
                        .TransitionTo(ModerationApproved),
                    elseBinder => elseBinder
                        .ThenAsync(async context =>
                        {
                            var provider = context.GetPayload<IServiceProvider>();
                            var sender = provider.GetRequiredService<ISender>();

                            await sender.Send(
                                new RejectReviewCommand(context.Saga.CorrelationId),
                                context.CancellationToken);
                        })
                        .TransitionTo(ModerationRejected))
        );

        During(ValidationFailed,
            Ignore(ReviewValidationRequested),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            Ignore(ValidationsCompleted),
            Ignore(ReviewModeratedEvent));

        During(ModerationApproved,
            Ignore(ReviewValidationRequested),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            Ignore(ValidationsCompleted),
            Ignore(ReviewModeratedEvent));

        During(ModerationRejected,
            Ignore(ReviewValidationRequested),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            Ignore(ValidationsCompleted),
            Ignore(ReviewModeratedEvent));
    }
}
