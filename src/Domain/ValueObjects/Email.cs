using System.ComponentModel.DataAnnotations;

namespace Mind_Manager.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        var emailValidator = new EmailAddressAttribute();
        if (!emailValidator.IsValid(value))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value.ToLower();
    }

    public static implicit operator string(Email email) => email.Value;
    public static explicit operator Email(string value) => new Email(value);

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => obj is Email email && Equals(email);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Value;
}
