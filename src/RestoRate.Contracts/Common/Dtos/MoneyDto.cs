namespace RestoRate.Contracts.Common.Dtos;

/// <summary>
/// Представляет денежную сумму в минорных единицах и код валюты.
/// <para>AmountMinor — сумма в минорных единицах (например, копейки). Используйте целые числа, чтобы избежать проблем с округлением.</para>
/// <para>Currency — код ISO 4217, например "RUB".</para>
/// </summary>
public record MoneyDto(
    long AmountMinor,
    string Currency
);
