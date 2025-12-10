using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.Specification;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RestoRate.Restaurant.Domain.TagAggregate.Specifications;

public sealed class TagByNameSpec : Specification<Tag>, ISingleResultSpecification<Tag>
{
    public TagByNameSpec(string name)
    {
        var normalized = name.Trim().ToLowerInvariant();
        Query.Where(t => t.NormalizedName == normalized);
    }
}
