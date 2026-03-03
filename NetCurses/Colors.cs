namespace BCyT.NetCurses;

/// <summary>
/// Color pair management matching ncurses color API.
/// </summary>
public static class Colors
{
    public const short Black   = 0;
    public const short Red     = 1;
    public const short Green   = 2;
    public const short Yellow  = 3;
    public const short Blue    = 4;
    public const short Magenta = 5;
    public const short Cyan    = 6;
    public const short White   = 7;

    private static (short Fg, short Bg)[] _pairs = new (short, short)[256];
    private static bool _started;

    /// <summary>
    /// Initialize color support. Must be called before using color pairs.
    /// </summary>
    public static void StartColor()
    {
        _started = true;
        // Pair 0 defaults to white on black
        _pairs[0] = (White, Black);
    }

    /// <summary>
    /// Define a color pair with the given foreground and background.
    /// </summary>
    public static void InitPair(int pair, short fg, short bg)
    {
        if (!_started)
            throw new CursesException("StartColor() must be called before InitPair()");
        if (pair < 1 || pair > 255)
            throw new CursesException("Color pair number must be between 1 and 255");
        _pairs[pair] = (fg, bg);
    }

    /// <summary>
    /// Get the foreground and background colors for a pair number.
    /// </summary>
    internal static (short Fg, short Bg) GetPair(int pair)
    {
        if (pair < 0 || pair > 255)
            return (White, Black);
        return _pairs[pair];
    }

    /// <summary>
    /// Returns true if the terminal supports colors.
    /// </summary>
    public static bool HasColors() => true; // Always true for VT-capable terminals

    internal static void Reset()
    {
        _started = false;
        _pairs = new (short, short)[256];
    }
}
