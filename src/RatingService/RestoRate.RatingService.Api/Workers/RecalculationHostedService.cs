using Mediator;

using Microsoft.Extensions.Options;

using RestoRate.RatingService.Application.Configurations;
using RestoRate.RatingService.Application.UseCases.Rating.Recalculate;
using RestoRate.RatingService.Domain.Interfaces;

namespace RestoRate.RatingService.Api.Workers;

internal sealed class RecalculationHostedService(
    IServiceScopeFactory scopeFactory,
    IRatingRecalculationDebouncer debouncer,
    ILogger<RecalculationHostedService> logger,
    IOptionsMonitor<RatingServiceOptions> options)
    : BackgroundService
{
    private static readonly Action<ILogger, Exception?> LogDueRecalculationProcessingFailed =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(1, nameof(LogDueRecalculationProcessingFailed)),
            "Failed to process due restaurant rating recalculations.");

    private const int BatchSize = 32;

    private TimeSpan DebounceWindow => options.CurrentValue.DebounceWindow;

    private TimeSpan PollInterval => TimeSpan.FromMilliseconds(
        Math.Clamp((int)(DebounceWindow.TotalMilliseconds / 2), 25, 250));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var dueRestaurantIds = await debouncer.GetDueRestaurantIdsAsync(
                    DateTimeOffset.UtcNow,
                    BatchSize,
                    stoppingToken);

                if (dueRestaurantIds.Count == 0)
                {
                    await Task.Delay(PollInterval, stoppingToken);
                    continue;
                }

                foreach (var restaurantId in dueRestaurantIds)
                {
                    using var scope = scopeFactory.CreateScope();
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                    await sender.Send(new RecalculateRatingCommand(restaurantId), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogDueRecalculationProcessingFailed(logger, ex);
                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }
}
