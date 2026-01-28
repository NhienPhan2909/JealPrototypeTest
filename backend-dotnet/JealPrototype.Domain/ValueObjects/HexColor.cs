using System.Text.RegularExpressions;

namespace JealPrototype.Domain.ValueObjects;

public class HexColor : IEquatable<HexColor>
{
    private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);

    public string Value { get; }

    private HexColor(string value)
    {
        Value = value;
    }

    public static HexColor Create(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException("Color cannot be empty", nameof(color));

        if (!HexColorRegex.IsMatch(color))
            throw new ArgumentException("Invalid hex color format. Use #RRGGBB or #RGB", nameof(color));

        return new HexColor(color);
    }

    public bool Equals(HexColor? other) => other != null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is HexColor color && Equals(color);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Value;

    public static implicit operator string(HexColor color) => color.Value;
}
