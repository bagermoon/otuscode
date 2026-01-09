using ContractRestaurantStatus = RestoRate.Contracts.Restaurant.RestaurantStatus;
using DomainRestaurantStatus = RestoRate.SharedKernel.Enums.RestaurantStatus;

namespace RestoRate.RestaurantService.Application.Mappings;

public static class StatusMappingExtensions
{
    public static ContractRestaurantStatus ToContract(this DomainRestaurantStatus status) =>
        status.Name switch
        {
            nameof(DomainRestaurantStatus.Draft) => ContractRestaurantStatus.Draft,
            nameof(DomainRestaurantStatus.Published) => ContractRestaurantStatus.Published,
            nameof(DomainRestaurantStatus.Archived) => ContractRestaurantStatus.Archived,
            nameof(DomainRestaurantStatus.Rejected) => ContractRestaurantStatus.Rejected,
            nameof(DomainRestaurantStatus.OnModeration) => ContractRestaurantStatus.OnModeration,
            _ => ContractRestaurantStatus.Unknown
        };

    public static SharedKernel.Enums.RestaurantStatus ToDomain(this Contracts.Restaurant.RestaurantStatus contractStatus) =>
        contractStatus switch
        {
            ContractRestaurantStatus.Draft => DomainRestaurantStatus.Draft,
            ContractRestaurantStatus.Published => DomainRestaurantStatus.Published,
            ContractRestaurantStatus.Archived => DomainRestaurantStatus.Archived,
            ContractRestaurantStatus.Rejected => DomainRestaurantStatus.Rejected,
            ContractRestaurantStatus.OnModeration => DomainRestaurantStatus.OnModeration,
            _ => DomainRestaurantStatus.Draft // default/fallback for Unknown
        };
}