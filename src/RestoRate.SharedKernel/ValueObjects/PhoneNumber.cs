using System.Text.RegularExpressions;

using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace RestoRate.SharedKernel.ValueObjects;

public partial class PhoneNumber : ValueObject
{
    public string OperatorCode { get; }
    public string Number { get; }
    public string? Extension { get; }

    [GeneratedRegex(@"^\d{7,15}$")]
    private static partial Regex OnlyDigitsRegex();

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigitsRegex();

    public PhoneNumber(string operatorCode, string number, string? extension = null)
    {
        OperatorCode = Guard.Against.NullOrWhiteSpace(operatorCode, nameof(operatorCode));
        Number = ValidatePhoneNumber(number);
        Extension = extension;
    }

    private static string ValidatePhoneNumber(string number)
    {
        var input = Guard.Against.NullOrWhiteSpace(number, nameof(number));
        var cleaned = NonDigitsRegex().Replace(input, string.Empty);

        if (!OnlyDigitsRegex().IsMatch(cleaned))
            throw new ArgumentException("Номер должен содержать только цифры (от 7 до 15)", nameof(number));

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
