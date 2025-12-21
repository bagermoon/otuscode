using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

using RestoRate.SharedKernel.Enums;

namespace RestoRate.RestaurantService.Domain.RestaurantAggregate;

public class RestaurantCuisineType : EntityBase<Guid>
{
    public Guid RestaurantId { get; private set; }
    public CuisineType CuisineType { get; private set; } = default!;

    private RestaurantCuisineType() { }

    internal RestaurantCuisineType(Guid restaurantId, CuisineType cuisineType)
    {
        RestaurantId = restaurantId;
        CuisineType = Guard.Against.Null(cuisineType, nameof(cuisineType));
    }
}
