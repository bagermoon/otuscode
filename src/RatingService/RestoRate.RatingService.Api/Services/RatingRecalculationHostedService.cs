using Mediator;

using RestoRate.RatingService.Application.UseCases.Rating.Recalculate;
using RestoRate.RatingService.Domain.Interfaces;

namespace RestoRate.RatingService.Api.Services;

internal sealed class RatingRecalculationHostedService(
    IServiceScopeFactory scopeFactory,
    IRatingRecalculationDebouncer debouncer,
    ILogger<RatingRecalculationHostedService> logger,
    TimeSpan debounceWindow)
    : BackgroundService
{
    private static readonly TimeSpan DefaultDebounceWindow = TimeSpan.FromSeconds(1);
    private static readonly Action<ILogger, Exception?> LogDueRecalculationProcessingFailed =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(1, nameof(LogDueRecalculationProcessingFailed)),
            "Failed to process due restaurant rating recalculations.");

    private const int BatchSize = 32;

    private readonly TimeSpan _debounceWindow = debounceWindow > TimeSpan.Zero
        ? debounceWindow
        : DefaultDebounceWindow;

    private TimeSpan PollInterval => TimeSpan.FromMilliseconds(
        Math.Clamp((int)(_debounceWindow.TotalMilliseconds / 2), 25, 250));

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