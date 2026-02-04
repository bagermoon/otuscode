using Ardalis.Result;
using Ardalis.Specification;

using Microsoft.EntityFrameworkCore;

using RestoRate.BuildingBlocks.Data.Repository;
using RestoRate.ReviewService.Domain.Interfaces;
using RestoRate.ReviewService.Domain.ReviewAggregate;
using RestoRate.ReviewService.Domain.UserReferenceAggregate;
using RestoRate.ReviewService.Infrastructure.Data;
using RestoRate.SharedKernel.Filters;
namespace RestoRate.ReviewService.Infrastructure.Repositories;

public class ReviewRepository(ReviewDbContext context)
    : Repository<Review, ReviewDbContext>(context), IReviewRepository
{
    public override async Task<Review?> FirstOrDefaultAsync(ISpecification<Review> specification, CancellationToken cancellationToken = default)
    {
        var context = (ReviewDbContext)DbContext;
        var asNoTracking = specification.AsNoTracking;
        var preTrackedReviewIds = CapturePreTrackedReviewIds(context, asNoTracking);
        BeginTrackingIfNeeded(specification, asNoTracking);

        Review? review;
        try
        {
            review = await base.FirstOrDefaultAsync(specification, cancellationToken);
        }
        finally
        {
            RestoreNoTrackingIfNeeded(specification, asNoTracking);
        }

        if (review is null)
            return null;

        if (review.UserId == Guid.Empty)
            return review;

        var userIds = new[] { review.UserId };
        var userIdsSet = userIds.ToHashSet();
        var preTrackedUserIds = CapturePreTrackedUserIds(context, asNoTracking, userIdsSet);

        await LoadUsersAsync(context, userIds, cancellationToken);

        DetachIfNoTracking(
            context,
            asNoTracking,
            preTrackedReviewIds,
            preTrackedUserIds,
            reviewsToDetach: new[] { review.Id },
            userIdsToDetach: userIdsSet);

        return review;
    }

    public override async Task<List<Review>> ListAsync(ISpecification<Review> specification, CancellationToken cancellationToken = default)
    {
        var context = (ReviewDbContext)DbContext;
        var asNoTracking = specification.AsNoTracking;
        var preTrackedReviewIds = CapturePreTrackedReviewIds(context, asNoTracking);
        BeginTrackingIfNeeded(specification, asNoTracking);

        List<Review> reviews;
        try
        {
            reviews = await base.ListAsync(specification, cancellationToken);
        }
        finally
        {
            RestoreNoTrackingIfNeeded(specification, asNoTracking);
        }

        if (reviews.Count == 0)
            return reviews;

        var userIds = reviews.Select(r => r.UserId).Distinct().ToArray();

        if (userIds.Length == 0)
            return reviews;

        var userIdsSet = userIds.ToHashSet();
        var preTrackedUserIds = CapturePreTrackedUserIds(context, asNoTracking, userIdsSet);

        await LoadUsersAsync(context, userIds, cancellationToken);

        DetachIfNoTracking(
            context,
            asNoTracking,
            preTrackedReviewIds,
            preTrackedUserIds,
            reviewsToDetach: reviews.Select(r => r.Id),
            userIdsToDetach: userIdsSet);

        return reviews;
    }

    public override async Task<PagedResult<List<Review>>> ListAsync(ISpecification<Review> specification, BaseFilter filter, CancellationToken cancellationToken = default)
    {
        var context = (ReviewDbContext)DbContext;
        var asNoTracking = specification.AsNoTracking;
        var preTrackedReviewIds = CapturePreTrackedReviewIds(context, asNoTracking);
        BeginTrackingIfNeeded(specification, asNoTracking);

        PagedResult<List<Review>> pagedResult;
        try
        {
            pagedResult = await base.ListAsync(specification, filter, cancellationToken);
        }
        finally
        {
            RestoreNoTrackingIfNeeded(specification, asNoTracking);
        }

        if (pagedResult.PagedInfo.TotalRecords == 0)
            return pagedResult;

        var reviews = pagedResult.Value;

        var userIds = reviews.Select(r => r.UserId).Distinct().ToArray();
        if (userIds.Length == 0)
            return pagedResult;

        var userIdsSet = userIds.ToHashSet();
        var preTrackedUserIds = CapturePreTrackedUserIds(context, asNoTracking, userIdsSet);

        await LoadUsersAsync(context, userIds, cancellationToken);

        DetachIfNoTracking(
            context,
            asNoTracking,
            preTrackedReviewIds,
            preTrackedUserIds,
            reviewsToDetach: reviews.Select(r => r.Id),
            userIdsToDetach: userIdsSet);

        return pagedResult;
    }

    private static void BeginTrackingIfNeeded(ISpecification<Review> specification, bool asNoTracking)
    {
        if (asNoTracking)
        {
            specification.Query.AsTracking();
        }
    }

    private static void RestoreNoTrackingIfNeeded(ISpecification<Review> specification, bool asNoTracking)
    {
        if (asNoTracking)
        {
            specification.Query.AsNoTracking();
        }
    }

    private static HashSet<Guid>? CapturePreTrackedReviewIds(ReviewDbContext context, bool asNoTracking)
    {
        return asNoTracking
            ? context.ChangeTracker.Entries<Review>().Select(e => e.Entity.Id).ToHashSet()
            : null;
    }

    private static HashSet<Guid>? CapturePreTrackedUserIds(ReviewDbContext context, bool asNoTracking, HashSet<Guid> relevantUserIds)
    {
        return asNoTracking
            ? context.ChangeTracker.Entries<UserReference>()
                .Select(e => e.Entity.Id)
                .Where(relevantUserIds.Contains)
                .ToHashSet()
            : null;
    }

    private static Task LoadUsersAsync(ReviewDbContext context, Guid[] userIds, CancellationToken cancellationToken)
    {
        if (userIds.Length == 0)
        {
            return Task.CompletedTask;
        }

        return context.UserReferences
            .Where(u => userIds.Contains(u.Id))
            .LoadAsync(cancellationToken);
    }

    private static void DetachIfNoTracking(
        ReviewDbContext context,
        bool asNoTracking,
        HashSet<Guid>? preTrackedReviewIds,
        HashSet<Guid>? preTrackedUserIds,
        IEnumerable<Guid> reviewsToDetach,
        HashSet<Guid> userIdsToDetach)
    {
        if (!asNoTracking)
            return;

        var reviewIdsSet = reviewsToDetach.ToHashSet();

        foreach (var entry in context.ChangeTracker.Entries<UserReference>()
                     .Where(e => userIdsToDetach.Contains(e.Entity.Id)))
        {
            if (preTrackedUserIds is not null && preTrackedUserIds.Contains(entry.Entity.Id))
                continue;

            entry.State = EntityState.Detached;
        }

        foreach (var entry in context.ChangeTracker.Entries<Review>()
                     .Where(e => reviewIdsSet.Contains(e.Entity.Id)))
        {
            if (preTrackedReviewIds is not null && preTrackedReviewIds.Contains(entry.Entity.Id))
                continue;

            entry.State = EntityState.Detached;
        }
    }
}
