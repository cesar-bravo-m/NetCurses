namespace BCyT.NetCurses;

/// <summary>
/// Attribute flag constants mirroring ncurses chtype bit layout.
/// Bits 8-15: color pair number. Bits 16+: attribute flags.
/// </summary>
public static class Attrs
{
    public const uint Normal    = 0x00000000;
    public const uint Standout  = 0x00010000;
    public const uint Underline = 0x00020000;
    public const uint Reverse   = 0x00040000;
    public const uint Blink     = 0x00080000;
    public const uint Dim       = 0x00100000;
    public const uint Bold      = 0x00200000;
    public const uint Invisible = 0x00400000;
    public const uint Italic    = 0x00800000;

    // Mask for color pair bits (bits 8-15)
    private const uint ColorMask = 0x0000FF00;
    private const int ColorShift = 8;

    /// <summary>
    /// Returns the attribute value encoding the given color pair number.
    /// </summary>
    public static uint ColorPair(int n) => (uint)(n & 0xFF) << ColorShift;

    /// <summary>
    /// Extracts the color pair number from an attribute value.
    /// </summary>
    public static int PairNumber(uint attr) => (int)((attr & ColorMask) >> ColorShift);
}
