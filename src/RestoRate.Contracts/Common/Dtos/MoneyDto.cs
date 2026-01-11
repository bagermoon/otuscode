namespace RestoRate.Contracts.Common.Dtos;

/// <summary>
/// Представляет денежную сумму и код валюты.
/// <para>Amount — сумма (например, рублях).</para>
/// <para>Currency — код ISO 4217, например "RUB".</para>
/// </summary>
public record MoneyDto(
    decimal Amount,
    string Currency
);
