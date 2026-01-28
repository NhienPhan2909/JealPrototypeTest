using System.Text.RegularExpressions;

namespace JealPrototype.Domain.ValueObjects;

public class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (email.Length > 255)
            throw new ArgumentException("Email must be 255 characters or less", nameof(email));

        return new Email(email);
    }

    public bool Equals(Email? other) => other != null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is Email email && Equals(email);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
