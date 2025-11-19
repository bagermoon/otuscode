using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace RestoRate.SharedKernel.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string OperatorCode { get; }
    public string Number { get; }
    public string? Extension { get; }

    public PhoneNumber(string operatorCode, string number, string? extension = null)
    {
        OperatorCode = Guard.Against.NullOrWhiteSpace(operatorCode, nameof(operatorCode));
        Number = ValidatePhoneNumber(number);
        Extension = extension;
    }

    private static string ValidatePhoneNumber(string number)
    {
        var cleaned = Guard.Against.NullOrWhiteSpace(number, nameof(number));

        // +7(XXX) XXX-XX-XX или +7 XXX XXX XX XX
        var pattern = @"^\+?(\d{1,3})?[-.\s]?(\(?)(\d{3})(\)?)[-.\s]?(\d{3})[-.\s]?(\d{2})[-.\s]?(\d{2})$";

        if (!Regex.IsMatch(cleaned, pattern))
            throw new ArgumentException("Неверный формат тел. номера. Верный формат: +7(XXX) XXX-XX-XX", nameof(number));

        return cleaned;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OperatorCode;
        yield return Number;
        yield return Extension ?? string.Empty;
    }

    public override string ToString() => Extension != null
        ? $"{OperatorCode}{Number} ext. {Extension}"
        : $"{OperatorCode}{Number}";
}
