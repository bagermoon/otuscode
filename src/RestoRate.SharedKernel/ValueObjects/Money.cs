using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace RestoRate.SharedKernel.ValueObjects;

public class Money : ValueObject
{
    public const string DefaultCurrency = "RUB";

    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = DefaultCurrency)
    {
        Amount = Guard.Against.Negative(amount, nameof(amount));
        Currency = Guard.Against.NullOrWhiteSpace(currency, nameof(currency)).ToUpper();

        ValidateCurrencyCode(currency);
    }

    private static void ValidateCurrencyCode(string code)
    {
        if (code.Length != 3)
            throw new ArgumentException("Код валюты должен содержать 3 символа", nameof(code));

        if (!code.All(char.IsLetter))
            throw new ArgumentException("Код валюты должен содержать только буквы", nameof(code));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public string GetCurrencySymbol() => Currency switch
    {
        "RUB" => "₽",
        "USD" => "$",
        "EUR" => "€",
        "CNY" => "¥",
        "KZT" => "₸",
        "BYN" => "Br",
        _ => Currency
    };

    public string GetCurrencyName() => Currency switch
    {
        "RUB" => "Российский рубль",
        "USD" => "Доллар США",
        "EUR" => "Евро",
        "CNY" => "Китайский юань",
        "KZT" => "Казахстанский тенге",
        "BYN" => "Беларусский рубль",
        _ => Currency
    };

    public override string ToString() => $"{GetCurrencySymbol()}{Amount:F2}";

    public string ToFormattedString() => $"{Amount:F2} {Currency} ({GetCurrencySymbol()})";
    public string ToShortString() => $"{GetCurrencySymbol()}{Amount:F2}";

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Невозможно складывать {Currency} и {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Невозможно вычитать {Currency} из {other.Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    public bool IsEqual(Money other)
    {
        if (Currency != other.Currency)
            return false;

        return Amount == other.Amount;
    }

    public bool IsZero() => Amount == 0;
    public bool IsNegative() => Amount < 0;
    public bool IsPositive() => Amount > 0;

    public Money Abs() => new Money(Math.Abs(Amount), Currency);
}
