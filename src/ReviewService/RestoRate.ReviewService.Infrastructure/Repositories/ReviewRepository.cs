using RestoRate.BuildingBlocks.Data.Repository;
using RestoRate.ReviewService.Infrastructure.Data;

using ReviewEntity = RestoRate.ReviewService.Domain.ReviewAggregate.Review;
namespace RestoRate.ReviewService.Infrastructure.Repositories;

public class ReviewRepository(ReviewDbContext context)
    : Repository<ReviewEntity, ReviewDbContext>(context)
{
}
