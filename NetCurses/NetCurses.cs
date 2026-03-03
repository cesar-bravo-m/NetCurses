namespace BCyT.NetCurses;

/// <summary>
/// Static API facade for ncurses-compatible TUI operations.
/// </summary>
public static class NetCurses
{
    private static Window? _stdscr;
    private static Screen? _screen;
    private static bool _initialized;
    private static readonly List<Window> _windows = new();

    /// <summary>
    /// The standard screen window (equivalent to ncurses stdscr).
    /// </summary>
    public static Window StdScr => _stdscr ?? throw new CursesException("InitScreen() not called");

    /// <summary>
    /// Number of rows in the terminal.
    /// </summary>
    public static int Lines => _screen?.Rows ?? Console.WindowHeight;

    /// <summary>
    /// Number of columns in the terminal.
    /// </summary>
    public static int Cols => _screen?.Cols ?? Console.WindowWidth;

    /// <summary>
    /// Get the internal screen for window refresh operations.
    /// </summary>
    internal static Screen GetScreen() =>
        _screen ?? throw new CursesException("InitScreen() not called");

    // --- Init / Teardown ---

    /// <summary>
    /// Initialize the curses library. Enables VT processing, switches to alternate screen,
    /// and creates the standard screen window.
    /// </summary>
    public static Window InitScreen()
    {
        if (_initialized)
            throw new CursesException("InitScreen() already called");

        AnsiBackend.Init();
        AnsiBackend.AlternateScreenEnter();
        AnsiBackend.HideCursor();
        AnsiBackend.ClearScreen();
        AnsiBackend.Flush();

        int rows = Console.WindowHeight;
        int cols = Console.WindowWidth;

        _screen = new Screen(rows, cols);
        _stdscr = new Window(rows, cols, 0, 0);
        _windows.Add(_stdscr);
        _initialized = true;

        InputDriver.Reset();

        return _stdscr;
    }

    /// <summary>
    /// Restore the terminal to its original state.
    /// </summary>
    public static void EndWin()
    {
        if (!_initialized) return;

        AnsiBackend.SetAttributes(Attrs.Normal);
        AnsiBackend.ShowCursor();
        AnsiBackend.AlternateScreenLeave();
        AnsiBackend.Flush();
        AnsiBackend.Shutdown();

        _stdscr = null;
        _screen = null;
        _windows.Clear();
        _initialized = false;
        InputDriver.Reset();
        Colors.Reset();
    }

    // --- Window Management ---

    /// <summary>
    /// Create a new window.
    /// </summary>
    public static Window NewWindow(int rows, int cols, int beginY, int beginX)
    {
        var win = new Window(rows, cols, beginY, beginX);
        _windows.Add(win);
        return win;
    }

    /// <summary>
    /// Delete a window.
    /// </summary>
    public static void DeleteWindow(Window win)
    {
        _windows.Remove(win);
    }

    /// <summary>
    /// Create a subwindow.
    /// </summary>
    public static Window SubWindow(Window parent, int rows, int cols, int beginY, int beginX)
    {
        var sub = parent.SubWindow(rows, cols, beginY, beginX);
        _windows.Add(sub);
        return sub;
    }

    /// <summary>
    /// Move a window to a new position.
    /// </summary>
    public static void MoveWindow(Window win, int y, int x) => win.MoveWindow(y, x);

    // --- StdScr delegation ---

    public static void Move(int y, int x) => StdScr.Move(y, x);
    public static void AddChar(int ch) => StdScr.AddChar(ch);
    public static void AddChar(System.Text.Rune ch) => StdScr.AddChar(ch);
    public static void AddString(string str) => StdScr.AddString(str);
    public static void Print(string format, params object[] args) => StdScr.Print(format, args);
    public static void InsertChar(System.Text.Rune ch) => StdScr.InsertChar(ch);
    public static void DeleteChar() => StdScr.DeleteChar();

    public static void MvAddChar(int y, int x, int ch) => StdScr.MvAddChar(y, x, ch);
    public static void MvAddChar(int y, int x, System.Text.Rune ch) => StdScr.MvAddChar(y, x, ch);
    public static void MvAddString(int y, int x, string str) => StdScr.MvAddString(y, x, str);
    public static void MvPrint(int y, int x, string format, params object[] args) => StdScr.MvPrint(y, x, format, args);

    public static void AttributeOn(uint attrs) => StdScr.AttributeOn(attrs);
    public static void AttributeOff(uint attrs) => StdScr.AttributeOff(attrs);
    public static void AttributeSet(uint attrs) => StdScr.AttributeSet(attrs);
    public static void Standout() => StdScr.Standout();
    public static void StandEnd() => StdScr.StandEnd();

    public static void Clear() => StdScr.Clear();
    public static void Erase() => StdScr.Erase();
    public static void ClearToEndOfLine() => StdScr.ClearToEndOfLine();
    public static void ClearToBottom() => StdScr.ClearToBottom();

    public static void Refresh() => StdScr.Refresh();
    public static int GetChar() => StdScr.GetChar();
    public static string GetString(int maxLen) => StdScr.GetString(maxLen);

    public static void ScrollOk(bool enable) => StdScr.ScrollOk(enable);

    // --- Screen Update ---

    /// <summary>
    /// Update the terminal from the virtual screen.
    /// </summary>
    public static void DoUpdate() => GetScreen().DoUpdate();

    // --- Color ---

    /// <summary>
    /// Initialize color support.
    /// </summary>
    public static void StartColor() => Colors.StartColor();

    /// <summary>
    /// Define a color pair.
    /// </summary>
    public static void InitPair(int pair, short fg, short bg) => Colors.InitPair(pair, fg, bg);

    /// <summary>
    /// Check if terminal supports colors.
    /// </summary>
    public static bool HasColors() => Colors.HasColors();

    /// <summary>
    /// Get the attribute value for a color pair number.
    /// </summary>
    public static uint ColorPair(int n) => Attrs.ColorPair(n);

    // --- Input Modes ---

    /// <summary>
    /// Enable raw mode (no line buffering, no signal processing).
    /// </summary>
    public static void Raw() => InputDriver.RawMode = true;

    /// <summary>
    /// Disable raw mode.
    /// </summary>
    public static void NoRaw() => InputDriver.RawMode = false;

    /// <summary>
    /// Enable cbreak mode (no line buffering, signals still processed).
    /// </summary>
    public static void CBreak() => InputDriver.CBreakMode = true;

    /// <summary>
    /// Disable cbreak mode.
    /// </summary>
    public static void NoCBreak() => InputDriver.CBreakMode = false;

    /// <summary>
    /// Enable echo mode.
    /// </summary>
    public static void Echo() => InputDriver.EchoMode = true;

    /// <summary>
    /// Disable echo mode.
    /// </summary>
    public static void NoEcho() => InputDriver.EchoMode = false;

    /// <summary>
    /// Set half-delay mode (tenths of seconds).
    /// </summary>
    public static void HalfDelay(int tenths) => InputDriver.TimeoutMs = tenths * 100;

    // --- Misc ---

    /// <summary>
    /// Sound the terminal bell.
    /// </summary>
    public static void Beep()
    {
        AnsiBackend.Bell();
        AnsiBackend.Flush();
    }

    /// <summary>
    /// Flash the screen (visual bell).
    /// </summary>
    public static void Flash() => AnsiBackend.Flash();

    /// <summary>
    /// Sleep for the given number of milliseconds.
    /// </summary>
    public static void Nap(int ms) => Thread.Sleep(ms);

    /// <summary>
    /// Set cursor visibility: 0=invisible, 1=normal, 2=very visible.
    /// </summary>
    public static void CursorSet(int visibility)
    {
        AnsiBackend.SetCursorVisibility(visibility);
        AnsiBackend.Flush();
    }

    /// <summary>
    /// Push a character back into the input buffer.
    /// </summary>
    public static void UngetChar(int ch) => InputDriver.UngetChar(ch);
}
