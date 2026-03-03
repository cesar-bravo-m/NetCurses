namespace BCyT.NetCurses;

/// <summary>
/// Key code constants matching ncurses KEY_* values.
/// </summary>
public static class Key
{
    public const int ErrKey    = -1;

    public const int Down      = 0x102;
    public const int Up        = 0x103;
    public const int Left      = 0x104;
    public const int Right     = 0x105;
    public const int Home      = 0x106;
    public const int Backspace = 0x107;

    public const int F1        = 0x109;
    public const int F2        = 0x10A;
    public const int F3        = 0x10B;
    public const int F4        = 0x10C;
    public const int F5        = 0x10D;
    public const int F6        = 0x10E;
    public const int F7        = 0x10F;
    public const int F8        = 0x110;
    public const int F9        = 0x111;
    public const int F10       = 0x112;
    public const int F11       = 0x113;
    public const int F12       = 0x114;

    public const int Delete    = 0x14A;
    public const int Insert    = 0x14B;

    public const int PageDown  = 0x152;
    public const int PageUp    = 0x153;

    public const int End       = 0x168;
    public const int Enter     = 0x157;

    public const int Resize    = 0x19A;
}
