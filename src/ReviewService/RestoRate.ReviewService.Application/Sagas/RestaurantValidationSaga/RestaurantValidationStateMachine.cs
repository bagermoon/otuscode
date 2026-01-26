using Ardalis.SharedKernel;

using MassTransit;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

using RestoRate.ReviewService.Application.Sagas.Messages;
using RestoRate.ReviewService.Application.UseCases.RestaurantReferences.RestaurantReferenceValidation;
using RestoRate.ReviewService.Domain.ReviewAggregate;
using RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;

namespace RestoRate.ReviewService.Application.Sagas.RestaurantValidationSaga;

public sealed class RestaurantValidationStateMachine : MassTransitStateMachine<RestaurantValidationState>
{
    public State Validating { get; private set; } = default!;
    public State Validated { get; private set; } = default!;
    public Event<ReviewValidationRequested> ReviewValidationRequested { get; private set; } = default!;
    public Event<RestaurantReferenceValidationStatus> RestaurantReferenceValidation { get; private set; } = default!;
    public RestaurantValidationStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => ReviewValidationRequested, x =>
        {
            x.CorrelateById(context => context.Message.RestaurantId);
            x.SelectId(context => context.Message.RestaurantId);
        });

        Event(() => RestaurantReferenceValidation, x =>
        {
            x.CorrelateById(context => context.Message.RestaurantId);
        });

        Initially(
            When(ReviewValidationRequested)
                .Then(context =>
                {
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Validating)
                .ThenAsync(async context =>
                {
                    var provider = context.GetPayload<IServiceProvider>();
                    var sender = provider.GetRequiredService<ISender>();

                    await sender.Send(
                        new RestaurantReferenceValidationCommand(context.Message.RestaurantId),
                        context.CancellationToken);
                }));

        During(Validating,
            Ignore(ReviewValidationRequested),
            When(RestaurantReferenceValidation)
                .Then(context =>
                {
                    context.Saga.ValidatedAt = DateTime.UtcNow;
                    context.Saga.IsValid = context.Message.IsValid;
                })
                .TransitionTo(Validated)
                .ThenAsync(async context =>
                {
                    var provider = context.GetPayload<IServiceProvider>();
                    var reviewRepository = provider.GetRequiredService<IRepository<Review>>();

                    var pendingReviews = await reviewRepository.ListAsync(
                        new GetPendingReviewsSpec(context.Saga.CorrelationId),
                        context.CancellationToken);

                    foreach (var review in pendingReviews)
                    {
                        await context.Publish(
                            new RestaurantValidationCompleted(
                                ReviewId: review.Id,
                                RestaurantId: context.Saga.CorrelationId,
                                ValidatedAt: context.Saga.ValidatedAt ?? DateTime.UtcNow,
                                IsValid: context.Saga.IsValid ?? false
                            ),
                            publishContext => publishContext.CorrelationId = review.Id);
                    }
                })
                .Finalize());

        During(Validated,
            Ignore(ReviewValidationRequested),
            Ignore(RestaurantReferenceValidation));

        SetCompletedWhenFinalized();
    }
}
