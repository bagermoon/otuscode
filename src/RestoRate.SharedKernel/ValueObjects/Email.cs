using System.Net.Mail;

using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace RestoRate.SharedKernel.ValueObjects;

public class Email : ValueObject
{
    public string Address { get; }

    public Email(string address)
    {
        Address = ValidateEmail(address);
    }

    private static string ValidateEmail(string email)
    {
        var trimmed = Guard.Against.NullOrWhiteSpace(email, nameof(email)).Trim();

        try
        {
            var addr = new MailAddress(trimmed);
            return addr.Address;
        }
        catch (FormatException)
        {
            throw new ArgumentException($"Неверный формат Эл. адреса: {trimmed}", nameof(email));
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address.ToLowerInvariant();
    }

    public override string ToString() => Address;

    /// <summary> Вытаскивает первую часть адреса(до @) </summary>
    public string GetLocalPart() => Address.Split('@')[0];
    /// <summary> Вытаскивает вторую часть адреса(после @) </summary>
    public string GetDomain() => Address.Split('@')[1];
}
