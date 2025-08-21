namespace GK.Talks.Core.ValueObjects;
public record FullName(string FirstName, string LastName)
{
    public static FullName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));
        return new FullName(firstName, lastName);
    }
}