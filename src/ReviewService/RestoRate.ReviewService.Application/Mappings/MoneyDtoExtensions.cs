using NodaMoney;

using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.ReviewService.Application.Mappings;

public static class MoneyDtoExtensions
{
    public static Money ToDomainMoney(this MoneyDto dto)
    {
        var currency = Currency.FromCode(dto.Currency);
        return new Money(dto.Amount, currency);
    }

    public static MoneyDto ToDto(this Money money)
        => new(money.Amount, money.Currency.Code);
}
