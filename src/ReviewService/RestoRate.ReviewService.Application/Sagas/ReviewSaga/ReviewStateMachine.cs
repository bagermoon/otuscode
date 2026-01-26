using MassTransit;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.Sagas.Messages;

namespace RestoRate.ReviewService.Application.Sagas.ReviewSaga;

public class ReviewStateMachine : MassTransitStateMachine<ReviewState>
{
    public State Validating { get; private set; } = default!;

    public State ValidationOk { get; private set; } = default!;
    public State ValidationFailed { get; private set; } = default!;

    public Event<ReviewAddedEvent> ReviewAdded { get; private set; } = default!;
    public Event<RestaurantValidationCompleted> RestaurantValidationCompleted { get; private set; } = default!;
    public Event<UserValidationCompleted> UserValidationCompleted { get; private set; } = default!;
    public Event ValidationsCompleted { get; private set; } = default!;
    public ReviewStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => ReviewAdded, x =>
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

        CompositeEvent(
            () => ValidationsCompleted,
            x => x.ValidationCompleted,
            RestaurantValidationCompleted,
            UserValidationCompleted);

        Initially(
            When(ReviewAdded)
                .Then(context =>
                {
                    context.Saga.RestaurantId = context.Message.RestaurantId;
                    context.Saga.UserId = context.Message.AuthorId;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .Publish(context => new ReviewValidationRequested(
                    RestaurantId: context.Saga.RestaurantId,
                    ReviewId: context.Saga.CorrelationId,
                    UserId: context.Saga.UserId))
                .TransitionTo(Validating));

        During(Validating,
            Ignore(ReviewAdded),
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
                    thenBinder => thenBinder.TransitionTo(ValidationOk),
                    elseBinder => elseBinder.TransitionTo(ValidationFailed))
        );

        During(ValidationOk,
            Ignore(ReviewAdded),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            Ignore(ValidationsCompleted));

        During(ValidationFailed,
            Ignore(ReviewAdded),
            Ignore(RestaurantValidationCompleted),
            Ignore(UserValidationCompleted),
            Ignore(ValidationsCompleted));
    }
}
