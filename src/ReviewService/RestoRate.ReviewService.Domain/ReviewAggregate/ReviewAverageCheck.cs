using NodaMoney;

namespace RestoRate.ReviewService.Domain.ReviewAggregate;

public sealed class ReviewAverageCheck
{
    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    private ReviewAverageCheck()
    {
    }

    private ReviewAverageCheck(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static ReviewAverageCheck? FromMoney(Money? money)
    {
        return money is null
            ? null
            : new ReviewAverageCheck(money.Value.Amount, money.Value.Currency.Code);
    }

    public Money ToMoney()
    {
        return new Money(Amount, NodaMoney.Currency.FromCode(Currency));
    }
}
