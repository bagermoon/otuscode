using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate;

public class RestaurantTag : EntityBase<Guid>
{
    public Guid RestaurantId { get; private set; }
    public Tag Tag { get; private set; } = default!;

    private RestaurantTag() { }

    internal RestaurantTag(Guid restaurantId, Tag tag)
    {
        RestaurantId = restaurantId;
        Tag = Guard.Against.Null(tag, nameof(tag));
    }
}
