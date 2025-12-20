using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace RestoRate.RestaurantService.Domain.RestaurantAggregate;

public class RestaurantImage : EntityBase<Guid>
{
    public Guid RestaurantId { get; private set; }
    /// <summary> Ссылка на изображение </summary>
    public string Url { get; private set; } = default!;
    /// <summary> Запасной вариант </summary>
    public string? AltText { get; private set; }
    /// <summary> Порядок отображения </summary>
    public int DisplayOrder { get; private set; }
    /// <summary> Основное изображение </summary>
    public bool IsPrimary { get; private set; }

    private RestaurantImage() { }
    internal RestaurantImage(
        Guid restaurantId,
        string url,
        string? altText = null,
        int displayOrder = 0,
        bool isPrimary = false)
    {
        Id = Guid.NewGuid();
        RestaurantId = restaurantId;
        Url = Guard.Against.NullOrEmpty(url, nameof(url));
        AltText = altText;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
    }

    internal void UpdateUrl(string url)
    {
        Url = Guard.Against.NullOrEmpty(url, nameof(url));
    }

    internal void UpdateDisplayOrder(int order)
    {
        DisplayOrder = Guard.Against.Negative(order, nameof(order));
    }

    internal void MarkAsPrimary()
    {
        IsPrimary = true;
    }

    internal void UnmarkAsPrimary()
    {
        IsPrimary = false;
    }
}
