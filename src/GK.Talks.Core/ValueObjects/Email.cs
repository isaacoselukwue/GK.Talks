using System.Text.RegularExpressions;

namespace GK.Talks.Core.ValueObjects;
public record Email(string Address)
{
    public static Email Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address) || !Regex.IsMatch(address, @"^(.+)@(.+)$"))
            throw new ArgumentException("Invalid email format.", nameof(address));
        return new Email(address);
    }
}