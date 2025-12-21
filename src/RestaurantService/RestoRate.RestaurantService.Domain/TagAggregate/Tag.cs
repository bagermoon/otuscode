using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace RestoRate.RestaurantService.Domain.TagAggregate;

public class Tag : EntityBase<Guid>, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string NormalizedName { get; private set; } = default!;

    private Tag() { }

    public Tag(string name)
    {
        Id = Guid.NewGuid();
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        NormalizedName = name.Trim().ToLowerInvariant();
    }

    public void UpdateName(string newName)
    {
        Name = Guard.Against.NullOrEmpty(newName, nameof(newName));
        NormalizedName = newName.Trim().ToLowerInvariant();
    }
}
