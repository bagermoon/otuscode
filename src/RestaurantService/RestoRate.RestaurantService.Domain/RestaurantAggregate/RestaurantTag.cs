using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

using RestoRate.RestaurantService.Domain.TagAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.RestaurantService.Domain.RestaurantAggregate;

public class RestaurantTag : EntityBase<Guid>
{
    public Guid RestaurantId { get; private set; }
    public Guid TagId { get; private set; } = default!;
    public Tag Tag { get; private set; } = default!;

    private RestaurantTag() { }

    internal RestaurantTag(Guid restaurantId, Guid tagId)
    {
        Id = Guid.NewGuid();
        RestaurantId = Guard.Against.Default(restaurantId, nameof(restaurantId));
        TagId = Guard.Against.Default(tagId, nameof(tagId));
    }
}
