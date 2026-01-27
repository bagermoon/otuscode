using Ardalis.SharedKernel;

using MassTransit;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.UseCases.UserReferences.UserReferenceValidation;
using RestoRate.ReviewService.Domain.ReviewAggregate;
using RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;

namespace RestoRate.ReviewService.Application.Sagas.UserValidationSaga;

public sealed class UserValidationStateMachine : MassTransitStateMachine<UserValidationState>
{
    public State Validating { get; private set; } = default!;
    public State Validated { get; private set; } = default!;

    public Event<ReviewValidationRequested> ReviewValidationRequested { get; private set; } = default!;
    public Event<UserReferenceValidationStatus> UserReferenceValidation { get; private set; } = default!;

    public UserValidationStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => ReviewValidationRequested, x =>
        {
            x.CorrelateById(context => context.Message.UserId);
            x.SelectId(context => context.Message.UserId);
        });

        Event(() => UserReferenceValidation, x =>
        {
            x.CorrelateById(context => context.Message.UserId);
        });

        Initially(
            When(ReviewValidationRequested)
                .Then(context => { context.Saga.CreatedAt = DateTime.UtcNow; })
                .TransitionTo(Validating)
                .ThenAsync(async context =>
                {
                    var provider = context.GetPayload<IServiceProvider>();
                    var sender = provider.GetRequiredService<ISender>();

                    await sender.Send(
                        new UserReferenceValidationCommand(context.Message.UserId),
                        context.CancellationToken);
                }));

        During(Validating,
            Ignore(ReviewValidationRequested),
            When(UserReferenceValidation)
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
                        new GetPendingReviewsByUserSpec(context.Saga.CorrelationId),
                        context.CancellationToken);

                    foreach (var review in pendingReviews)
                    {
                        await context.Publish(
                            new UserValidationCompleted(
                                ReviewId: review.Id,
                                UserId: context.Saga.CorrelationId,
                                ValidatedAt: context.Saga.ValidatedAt ?? DateTime.UtcNow,
                                IsValid: context.Saga.IsValid ?? false
                            ),
                            publishContext => publishContext.CorrelationId = review.Id);
                    }
                })
                .Finalize());

        During(Validated,
            Ignore(ReviewValidationRequested),
            Ignore(UserReferenceValidation));

        SetCompletedWhenFinalized();
    }
}
