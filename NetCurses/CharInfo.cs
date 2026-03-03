using System.Text;

namespace BCyT.NetCurses;

/// <summary>
/// Represents a single cell in a window or screen buffer.
/// </summary>
public struct CharInfo : IEquatable<CharInfo>
{
    public Rune Character;
    public uint Attributes;

    public static readonly CharInfo Blank = new() { Character = new Rune(' '), Attributes = Attrs.Normal };

    public CharInfo(Rune character, uint attributes)
    {
        Character = character;
        Attributes = attributes;
    }

    public bool Equals(CharInfo other) =>
        Character == other.Character && Attributes == other.Attributes;

    public override bool Equals(object? obj) =>
        obj is CharInfo other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Character, Attributes);

    public static bool operator ==(CharInfo left, CharInfo right) => left.Equals(right);
    public static bool operator !=(CharInfo left, CharInfo right) => !left.Equals(right);
}
