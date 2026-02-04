using Ardalis.GuardClauses;
using Ardalis.Result;

namespace RestoRate.ReviewService.Application.Mappings;

public static class PagedResultContractExtensions
{
    public static RestoRate.Contracts.Common.PagedResult<TDestination> ToContractPagedResult<TSource, TDestination>(
        this Ardalis.Result.PagedResult<List<TSource>> pagedResult,
        Func<TSource, TDestination> map)
    {
        Guard.Against.Null(map, nameof(map));

        var items = (pagedResult.Value ?? [])
            .Select(map)
            .ToList();

        return new RestoRate.Contracts.Common.PagedResult<TDestination>(
            items,
            checked((int)pagedResult.PagedInfo.TotalRecords),
            checked((int)pagedResult.PagedInfo.PageNumber),
            checked((int)pagedResult.PagedInfo.PageSize));
    }
}
