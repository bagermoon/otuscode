using RestoRate.BuildingBlocks.Data.Repository;
using RestoRate.Review.Infrastructure.Data;

using ReviewEntity = RestoRate.Review.Domain.ReviewAggregate.Review;
namespace RestoRate.Review.Infrastructure.Repositories;

public class ReviewRepository(ReviewDbContext context)
    : Repository<ReviewEntity, ReviewDbContext>(context)
{
}
