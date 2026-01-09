
using Ardalis.Result;

using Mediator;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.RatingChange;

public sealed class RatingChangeHandler : ICommandHandler<RatingChangeCommand, Result>
{
    public async ValueTask<Result> Handle(RatingChangeCommand command, CancellationToken cancellationToken)
    {
        return Result.Success();
    }
}