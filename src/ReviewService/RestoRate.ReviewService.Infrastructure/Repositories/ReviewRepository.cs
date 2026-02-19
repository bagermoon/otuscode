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
        {
            DetachIfNoTracking(
                context,
                asNoTracking,
                preTrackedReviewIds,
                preTrackedUserIds: null,
                reviewsToDetach: new[] { review.Id },
                userIdsToDetach: new HashSet<Guid>());

            return review;
        }

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

        var userIds = reviews.Select(r => r.UserId).Where(id => id != Guid.Empty).Distinct().ToArray();

        if (userIds.Length == 0)
        {
            DetachIfNoTracking(
                context,
                asNoTracking,
                preTrackedReviewIds,
                preTrackedUserIds: null,
                reviewsToDetach: reviews.Select(r => r.Id),
                userIdsToDetach: new HashSet<Guid>());

            return reviews;
        }

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

    public override async Task<PagedResult<List<Review>>> ListPagedAsync(ISpecification<Review> specification, BaseFilter filter, CancellationToken cancellationToken = default)
    {
        var context = (ReviewDbContext)DbContext;
        var asNoTracking = specification.AsNoTracking;
        var preTrackedReviewIds = CapturePreTrackedReviewIds(context, asNoTracking);
        BeginTrackingIfNeeded(specification, asNoTracking);

        PagedResult<List<Review>> pagedResult;
        try
        {
            pagedResult = await base.ListPagedAsync(specification, filter, cancellationToken);
        }
        finally
        {
            RestoreNoTrackingIfNeeded(specification, asNoTracking);
        }

        if (pagedResult.PagedInfo.TotalRecords == 0)
            return pagedResult;

        var reviews = pagedResult.Value;

        var userIds = reviews.Select(r => r.UserId).Where(id => id != Guid.Empty).Distinct().ToArray();
        if (userIds.Length == 0)
        {
            DetachIfNoTracking(
                context,
                asNoTracking,
                preTrackedReviewIds,
                preTrackedUserIds: null,
                reviewsToDetach: reviews.Select(r => r.Id),
                userIdsToDetach: new HashSet<Guid>());

            return pagedResult;
        }

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
                .Where(e => relevantUserIds.Contains(e.Entity.Id))
                .Select(e => e.Entity.Id)
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

        var userRefsToDetach = context.ChangeTracker.Entries<UserReference>()
            .Where(e => userIdsToDetach.Contains(e.Entity.Id))
            .Where(e => preTrackedUserIds is null || !preTrackedUserIds.Contains(e.Entity.Id))
            .ToList();

        foreach (var entry in userRefsToDetach)
        {
            entry.State = EntityState.Detached;
        }

        var reviewIdsSet = reviewsToDetach.ToHashSet();
        var reviewRefsToDetach = context.ChangeTracker.Entries<Review>()
            .Where(e => reviewIdsSet.Contains(e.Entity.Id))
            .Where(e => preTrackedReviewIds is null || !preTrackedReviewIds.Contains(e.Entity.Id))
            .ToList();

        foreach (var entry in reviewRefsToDetach)
        {
            entry.State = EntityState.Detached;
        }
    }
}
