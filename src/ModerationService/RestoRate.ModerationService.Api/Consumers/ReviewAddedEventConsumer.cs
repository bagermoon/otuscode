using MassTransit;

using Microsoft.Extensions.Options;

using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Moderation.Events;
using RestoRate.ModerationService.Application.UseCases.Moderate;
using RestoRate.ModerationService.Infrastructure.Configurations;

using RestoRate.Contracts.Review.Events;

using Mediator;

namespace RestoRate.ModerationService.Api.Consumers
{
    public class ReviewAddedEventConsumer(
        ISender sender,
        IIntegrationEventBus integrationEventBus,
        IOptions<ModerationSettingsOptions> moderationSettings,
        ILogger<ReviewAddedEventConsumer> logger
    ) : IConsumer<ReviewAddedEvent>
    {
        public async Task Consume(ConsumeContext<ReviewAddedEvent> context)
        {
            var message = context.Message;

            var commandResult = await sender.Send(
                new ModerateCommand(message.ReviewId, message.Comment),
                context.CancellationToken);

            var approved = commandResult.IsSuccess;
            var reason = commandResult.IsSuccess ? null : string.Join("; ", commandResult.ValidationErrors.Select(e => e.ErrorMessage));

            var reviewModeratedEvent = new ReviewModeratedEvent(
                ReviewId: message.ReviewId,
                Approved: approved,
                Reason: reason,
                ModeratorId: moderationSettings.Value.SystemModeratorId);

            await integrationEventBus.PublishAsync(reviewModeratedEvent, context.CancellationToken);

            logger.LogModerated(message.ReviewId, approved);
        }
    }
}
