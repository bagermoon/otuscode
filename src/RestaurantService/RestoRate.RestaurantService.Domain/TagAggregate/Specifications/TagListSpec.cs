using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.Specification;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RestoRate.RestaurantService.Domain.TagAggregate.Specifications;

public sealed class TagListSpec : Specification<Tag>
{
    public TagListSpec()
    {
        Query.OrderBy(t => t.Name);
    }
}
