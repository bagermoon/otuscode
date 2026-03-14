namespace RestoRate.ReviewService.Application.Configurations;

public sealed class RestaurantProjectionOptions
{
    public const string SectionName = "RestaurantProjection";

    public static readonly TimeSpan DefaultFreshnessTtl = TimeSpan.FromMinutes(5);

    public TimeSpan FreshnessTtl { get; set; } = DefaultFreshnessTtl;
}
