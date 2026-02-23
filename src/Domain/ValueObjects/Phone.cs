using System.ComponentModel.DataAnnotations;

namespace Mind_Manager.Domain.ValueObjects;

public sealed class Phone : IEquatable<Phone>
{
    public string Value { get; }

    public Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone cannot be empty", nameof(value));

        var phoneValidator = new RegularExpressionAttribute(@"^\+?[1-9]\d{1,14}$");
        if (!phoneValidator.IsValid(value))
            throw new ArgumentException("Invalid phone format", nameof(value));

        Value = value;
    }

    public static implicit operator string(Phone phone) => phone.Value;
    public static explicit operator Phone(string value) => new Phone(value);

    public bool Equals(Phone? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => obj is Phone phone && Equals(phone);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
