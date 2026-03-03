using System.Runtime.InteropServices;
using System.Text;

namespace BCyT.NetCurses;

/// <summary>
/// ANSI/VT escape sequence engine for terminal output.
/// Uses P/Invoke to enable VT processing on Windows.
/// </summary>
internal static class AnsiBackend
{
    private const int STD_OUTPUT_HANDLE = -11;
    private const int STD_INPUT_HANDLE = -10;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;
    private const uint ENABLE_PROCESSED_INPUT = 0x0001;
    private const uint ENABLE_WINDOW_INPUT = 0x0008;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    private static readonly StringBuilder _buffer = new(4096);
    private static uint _originalOutMode;
    private static uint _originalInMode;
    private static bool _initialized;
    private static uint _lastAttrs = uint.MaxValue; // Force first SGR emission

    /// <summary>
    /// Enable VT processing on stdout and stdin.
    /// </summary>
    public static void Init()
    {
        if (_initialized) return;

        var hOut = GetStdHandle(STD_OUTPUT_HANDLE);
        GetConsoleMode(hOut, out _originalOutMode);
        SetConsoleMode(hOut, _originalOutMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);

        var hIn = GetStdHandle(STD_INPUT_HANDLE);
        GetConsoleMode(hIn, out _originalInMode);
        SetConsoleMode(hIn, (_originalInMode | ENABLE_VIRTUAL_TERMINAL_INPUT | ENABLE_WINDOW_INPUT) & ~ENABLE_PROCESSED_INPUT);

        Console.OutputEncoding = Encoding.UTF8;
        _initialized = true;
    }

    /// <summary>
    /// Restore original console modes.
    /// </summary>
    public static void Shutdown()
    {
        if (!_initialized) return;

        Flush();
        var hOut = GetStdHandle(STD_OUTPUT_HANDLE);
        SetConsoleMode(hOut, _originalOutMode);

        var hIn = GetStdHandle(STD_INPUT_HANDLE);
        SetConsoleMode(hIn, _originalInMode);

        _initialized = false;
        _lastAttrs = uint.MaxValue;
    }

    /// <summary>
    /// Flush buffered output to stdout.
    /// </summary>
    public static void Flush()
    {
        if (_buffer.Length > 0)
        {
            Console.Out.Write(_buffer.ToString());
            Console.Out.Flush();
            _buffer.Clear();
        }
    }

    private static void Emit(string s) => _buffer.Append(s);
    private static void Emit(char c) => _buffer.Append(c);

    /// <summary>
    /// Move cursor to row, col (0-based).
    /// </summary>
    public static void MoveCursor(int row, int col) =>
        _buffer.Append($"\x1b[{row + 1};{col + 1}H");

    public static void HideCursor() => Emit("\x1b[?25l");
    public static void ShowCursor() => Emit("\x1b[?25h");

    /// <summary>
    /// Set cursor visibility: 0=invisible, 1=normal, 2=very visible.
    /// </summary>
    public static void SetCursorVisibility(int visibility)
    {
        switch (visibility)
        {
            case 0:
                HideCursor();
                break;
            case 1:
                ShowCursor();
                // Normal cursor
                Emit("\x1b[3 q");
                break;
            case 2:
                ShowCursor();
                // Block cursor (very visible)
                Emit("\x1b[1 q");
                break;
        }
    }

    public static void ClearScreen() => Emit("\x1b[2J");

    public static void AlternateScreenEnter() => Emit("\x1b[?1049h");
    public static void AlternateScreenLeave() => Emit("\x1b[?1049l");

    public static void Bell() => Emit('\a');

    public static void Flash()
    {
        // Visual bell: reverse video, flush, brief pause, restore
        Emit("\x1b[?5h");
        Flush();
        Thread.Sleep(100);
        Emit("\x1b[?5l");
        Flush();
    }

    /// <summary>
    /// Emit SGR sequence for the given attribute flags + color pair.
    /// Only emits if attributes changed from last call.
    /// </summary>
    public static void SetAttributes(uint attrs)
    {
        if (attrs == _lastAttrs) return;
        _lastAttrs = attrs;

        _buffer.Append("\x1b[0"); // Reset first

        if ((attrs & Attrs.Bold) != 0) _buffer.Append(";1");
        if ((attrs & Attrs.Dim) != 0) _buffer.Append(";2");
        if ((attrs & Attrs.Italic) != 0) _buffer.Append(";3");
        if ((attrs & Attrs.Underline) != 0) _buffer.Append(";4");
        if ((attrs & Attrs.Blink) != 0) _buffer.Append(";5");
        if ((attrs & Attrs.Reverse) != 0) _buffer.Append(";7");
        if ((attrs & Attrs.Invisible) != 0) _buffer.Append(";8");
        if ((attrs & Attrs.Standout) != 0) _buffer.Append(";7"); // Standout = reverse

        int pairNum = Attrs.PairNumber(attrs);
        if (pairNum != 0)
        {
            var (fg, bg) = Colors.GetPair(pairNum);
            _buffer.Append($";{30 + fg};{40 + bg}");
        }

        _buffer.Append('m');
    }

    /// <summary>
    /// Force next SetAttributes call to emit SGR even if attrs haven't changed.
    /// </summary>
    public static void InvalidateAttributes() => _lastAttrs = uint.MaxValue;

    /// <summary>
    /// Write a single Rune to the buffer.
    /// </summary>
    public static void PutChar(Rune ch)
    {
        Span<char> buf = stackalloc char[2];
        int written = ch.EncodeToUtf16(buf);
        for (int i = 0; i < written; i++)
            _buffer.Append(buf[i]);
    }
}
