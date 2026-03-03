namespace BCyT.NetCurses;

/// <summary>
/// Input handling - maps ConsoleKeyInfo to ncurses key codes.
/// </summary>
internal static class InputDriver
{
    private static bool _rawMode;
    private static bool _cbreakMode;
    private static bool _echoMode = true;
    private static bool _keypadMode;
    private static int _timeoutMs = -1; // -1 = blocking
    private static readonly Queue<int> _ungetBuffer = new();

    public static bool RawMode { get => _rawMode; set { _rawMode = value; if (value) _cbreakMode = false; } }
    public static bool CBreakMode { get => _cbreakMode; set { _cbreakMode = value; if (value) _rawMode = false; } }
    public static bool EchoMode { get => _echoMode; set => _echoMode = value; }
    public static bool KeypadMode { get => _keypadMode; set => _keypadMode = value; }
    public static int TimeoutMs { get => _timeoutMs; set => _timeoutMs = value; }

    /// <summary>
    /// Push a character back into the input buffer.
    /// </summary>
    public static void UngetChar(int ch) => _ungetBuffer.Enqueue(ch);

    /// <summary>
    /// Read a single key, returning an ncurses-compatible key code.
    /// </summary>
    public static int GetChar()
    {
        // Check unget buffer first
        if (_ungetBuffer.Count > 0)
            return _ungetBuffer.Dequeue();

        // Handle timeout
        if (_timeoutMs == 0)
        {
            // Non-blocking
            if (!Console.KeyAvailable)
                return Key.ErrKey;
        }
        else if (_timeoutMs > 0)
        {
            // Timed wait
            int elapsed = 0;
            while (!Console.KeyAvailable && elapsed < _timeoutMs)
            {
                Thread.Sleep(10);
                elapsed += 10;
            }
            if (!Console.KeyAvailable)
                return Key.ErrKey;
        }
        // else _timeoutMs == -1: blocking (default Console.ReadKey behavior)

        var keyInfo = Console.ReadKey(true);

        // Echo if enabled and it's a printable character
        if (_echoMode && !char.IsControl(keyInfo.KeyChar) && keyInfo.KeyChar != '\0')
        {
            Console.Write(keyInfo.KeyChar);
        }

        // Map to ncurses key code if keypad mode is on
        if (_keypadMode)
        {
            int mapped = MapKey(keyInfo);
            if (mapped != Key.ErrKey)
                return mapped;
        }

        // Return the character value
        if (keyInfo.KeyChar != '\0')
            return keyInfo.KeyChar;

        // If no character and no keypad mapping, return error
        return Key.ErrKey;
    }

    /// <summary>
    /// Read a string with basic line editing. Returns the string entered.
    /// </summary>
    public static string GetString(Window window, int maxLen)
    {
        var sb = new System.Text.StringBuilder();
        bool oldEcho = _echoMode;
        int startY = window.CursorY;
        int startX = window.CursorX;

        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Enter)
                break;

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                    // Update display
                    int cx = startX + sb.Length;
                    window.Move(startY, cx);
                    window.AddChar(' ');
                    window.Move(startY, cx);
                    window.Refresh();
                }
                continue;
            }

            if (keyInfo.Key == ConsoleKey.Escape)
                break;

            if (!char.IsControl(keyInfo.KeyChar) && keyInfo.KeyChar != '\0' && sb.Length < maxLen)
            {
                sb.Append(keyInfo.KeyChar);
                window.AddChar(keyInfo.KeyChar);
                window.Refresh();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Map ConsoleKeyInfo to ncurses key code.
    /// </summary>
    private static int MapKey(ConsoleKeyInfo keyInfo)
    {
        return keyInfo.Key switch
        {
            ConsoleKey.UpArrow    => Key.Up,
            ConsoleKey.DownArrow  => Key.Down,
            ConsoleKey.LeftArrow  => Key.Left,
            ConsoleKey.RightArrow => Key.Right,
            ConsoleKey.Home       => Key.Home,
            ConsoleKey.End        => Key.End,
            ConsoleKey.PageUp     => Key.PageUp,
            ConsoleKey.PageDown   => Key.PageDown,
            ConsoleKey.Insert     => Key.Insert,
            ConsoleKey.Delete     => Key.Delete,
            ConsoleKey.Backspace  => Key.Backspace,
            ConsoleKey.Enter      => Key.Enter,
            ConsoleKey.F1         => Key.F1,
            ConsoleKey.F2         => Key.F2,
            ConsoleKey.F3         => Key.F3,
            ConsoleKey.F4         => Key.F4,
            ConsoleKey.F5         => Key.F5,
            ConsoleKey.F6         => Key.F6,
            ConsoleKey.F7         => Key.F7,
            ConsoleKey.F8         => Key.F8,
            ConsoleKey.F9         => Key.F9,
            ConsoleKey.F10        => Key.F10,
            ConsoleKey.F11        => Key.F11,
            ConsoleKey.F12        => Key.F12,
            _ => Key.ErrKey
        };
    }

    /// <summary>
    /// Reset all input state to defaults.
    /// </summary>
    internal static void Reset()
    {
        _rawMode = false;
        _cbreakMode = false;
        _echoMode = true;
        _keypadMode = false;
        _timeoutMs = -1;
        _ungetBuffer.Clear();
    }
}
