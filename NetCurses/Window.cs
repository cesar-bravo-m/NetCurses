using System.Text;

namespace BCyT.NetCurses;

/// <summary>
/// Represents an ncurses-compatible window with its own character buffer.
/// </summary>
public class Window
{
    private CharInfo[,] _buffer;
    private int _cursorY;
    private int _cursorX;
    private uint _attrs;
    private bool _scrollOk;
    private int _scrollTop;
    private int _scrollBottom;
    private bool _keypadMode;
    private int _timeoutMs = -1;

    // Window dimensions and position on the screen
    internal int MaxY { get; private set; }
    internal int MaxX { get; private set; }
    internal int BeginY { get; private set; }
    internal int BeginX { get; private set; }

    // Parent window reference for subwindows
    private Window? _parent;

    /// <summary>
    /// Current cursor row position.
    /// </summary>
    public int CursorY => _cursorY;

    /// <summary>
    /// Current cursor column position.
    /// </summary>
    public int CursorX => _cursorX;

    internal Window(int rows, int cols, int beginY, int beginX)
    {
        MaxY = rows;
        MaxX = cols;
        BeginY = beginY;
        BeginX = beginX;
        _buffer = new CharInfo[rows, cols];
        _scrollTop = 0;
        _scrollBottom = rows - 1;
        Erase();
    }

    /// <summary>
    /// Get the cell at the given position in the buffer.
    /// </summary>
    internal CharInfo GetCell(int row, int col) => _buffer[row, col];

    // --- Cursor ---

    /// <summary>
    /// Move the cursor to the given position.
    /// </summary>
    public void Move(int y, int x)
    {
        if (y >= 0 && y < MaxY && x >= 0 && x < MaxX)
        {
            _cursorY = y;
            _cursorX = x;
        }
    }

    /// <summary>
    /// Get the current cursor position.
    /// </summary>
    public void GetYX(out int y, out int x)
    {
        y = _cursorY;
        x = _cursorX;
    }

    // --- Attributes ---

    /// <summary>
    /// Turn on the given attribute flags.
    /// </summary>
    public void AttributeOn(uint attrs) => _attrs |= attrs;

    /// <summary>
    /// Turn off the given attribute flags.
    /// </summary>
    public void AttributeOff(uint attrs) => _attrs &= ~attrs;

    /// <summary>
    /// Set attributes to exactly the given value.
    /// </summary>
    public void AttributeSet(uint attrs) => _attrs = attrs;

    /// <summary>
    /// Enable standout mode (reverse video).
    /// </summary>
    public void Standout() => AttributeOn(Attrs.Standout);

    /// <summary>
    /// Disable standout mode.
    /// </summary>
    public void StandEnd() => AttributeOff(Attrs.Standout);

    // --- Output ---

    /// <summary>
    /// Add a character at the current cursor position.
    /// </summary>
    public void AddChar(int ch) => AddChar(new Rune(ch));

    /// <summary>
    /// Add a Rune at the current cursor position with optional attribute override.
    /// </summary>
    public void AddChar(Rune ch, uint attrs = 0)
    {
        uint a = attrs != 0 ? attrs : _attrs;

        if (ch.Value == '\n')
        {
            ClearToEndOfLine();
            AdvanceCursorNewline();
            return;
        }

        if (ch.Value == '\t')
        {
            int spaces = 8 - (_cursorX % 8);
            for (int i = 0; i < spaces; i++)
                AddChar(new Rune(' '), a);
            return;
        }

        if (_cursorY < MaxY && _cursorX < MaxX)
        {
            _buffer[_cursorY, _cursorX] = new CharInfo(ch, a);
            AdvanceCursor();
        }
    }

    /// <summary>
    /// Add a string at the current cursor position.
    /// </summary>
    public void AddString(string str)
    {
        foreach (var rune in str.EnumerateRunes())
            AddChar(rune);
    }

    /// <summary>
    /// Printf-style output at the current cursor position.
    /// </summary>
    public void Print(string format, params object[] args) =>
        AddString(string.Format(format, args));

    /// <summary>
    /// Insert a character at the cursor, shifting the rest of the line right.
    /// </summary>
    public void InsertChar(Rune ch)
    {
        if (_cursorY >= MaxY || _cursorX >= MaxX) return;

        // Shift characters right
        for (int x = MaxX - 1; x > _cursorX; x--)
            _buffer[_cursorY, x] = _buffer[_cursorY, x - 1];

        _buffer[_cursorY, _cursorX] = new CharInfo(ch, _attrs);
    }

    /// <summary>
    /// Delete the character at the cursor, shifting the rest of the line left.
    /// </summary>
    public void DeleteChar()
    {
        if (_cursorY >= MaxY || _cursorX >= MaxX) return;

        for (int x = _cursorX; x < MaxX - 1; x++)
            _buffer[_cursorY, x] = _buffer[_cursorY, x + 1];

        _buffer[_cursorY, MaxX - 1] = CharInfo.Blank;
    }

    // --- Mv* variants ---

    public void MvAddChar(int y, int x, int ch) { Move(y, x); AddChar(ch); }
    public void MvAddChar(int y, int x, Rune ch) { Move(y, x); AddChar(ch); }
    public void MvAddString(int y, int x, string str) { Move(y, x); AddString(str); }
    public void MvPrint(int y, int x, string format, params object[] args) { Move(y, x); Print(format, args); }

    // --- Clear ---

    /// <summary>
    /// Clear the entire window and move cursor to 0,0.
    /// </summary>
    public void Clear()
    {
        Erase();
        _cursorY = 0;
        _cursorX = 0;
    }

    /// <summary>
    /// Erase the entire window (fill with blanks) without moving cursor.
    /// </summary>
    public void Erase()
    {
        for (int r = 0; r < MaxY; r++)
            for (int c = 0; c < MaxX; c++)
                _buffer[r, c] = CharInfo.Blank;
    }

    /// <summary>
    /// Clear from cursor to end of line.
    /// </summary>
    public void ClearToEndOfLine()
    {
        for (int c = _cursorX; c < MaxX; c++)
            _buffer[_cursorY, c] = CharInfo.Blank;
    }

    /// <summary>
    /// Clear from cursor to bottom of window.
    /// </summary>
    public void ClearToBottom()
    {
        ClearToEndOfLine();
        for (int r = _cursorY + 1; r < MaxY; r++)
            for (int c = 0; c < MaxX; c++)
                _buffer[r, c] = CharInfo.Blank;
    }

    // --- Border / Box ---

    /// <summary>
    /// Draw a border around the window with the given characters.
    /// Pass 0 for any character to use the default.
    /// </summary>
    public void Border(Rune ls, Rune rs, Rune ts, Rune bs,
                       Rune tl, Rune tr, Rune bl, Rune br)
    {
        if (ls.Value == 0) ls = Acs.VLine;
        if (rs.Value == 0) rs = Acs.VLine;
        if (ts.Value == 0) ts = Acs.HLine;
        if (bs.Value == 0) bs = Acs.HLine;
        if (tl.Value == 0) tl = Acs.ULCorner;
        if (tr.Value == 0) tr = Acs.URCorner;
        if (bl.Value == 0) bl = Acs.LLCorner;
        if (br.Value == 0) br = Acs.LRCorner;

        int lastRow = MaxY - 1;
        int lastCol = MaxX - 1;

        // Corners
        _buffer[0, 0] = new CharInfo(tl, _attrs);
        _buffer[0, lastCol] = new CharInfo(tr, _attrs);
        _buffer[lastRow, 0] = new CharInfo(bl, _attrs);
        _buffer[lastRow, lastCol] = new CharInfo(br, _attrs);

        // Top and bottom lines
        for (int c = 1; c < lastCol; c++)
        {
            _buffer[0, c] = new CharInfo(ts, _attrs);
            _buffer[lastRow, c] = new CharInfo(bs, _attrs);
        }

        // Left and right lines
        for (int r = 1; r < lastRow; r++)
        {
            _buffer[r, 0] = new CharInfo(ls, _attrs);
            _buffer[r, lastCol] = new CharInfo(rs, _attrs);
        }
    }

    /// <summary>
    /// Draw a box with default line-drawing characters.
    /// </summary>
    public void Box(Rune vertChar, Rune horizChar)
    {
        Border(vertChar, vertChar, horizChar, horizChar,
               new Rune(0), new Rune(0), new Rune(0), new Rune(0));
    }

    /// <summary>
    /// Draw a horizontal line of length n starting at the cursor.
    /// </summary>
    public void HorizontalLine(Rune ch, int n)
    {
        if (ch.Value == 0) ch = Acs.HLine;
        for (int i = 0; i < n && (_cursorX + i) < MaxX; i++)
            _buffer[_cursorY, _cursorX + i] = new CharInfo(ch, _attrs);
    }

    /// <summary>
    /// Draw a vertical line of length n starting at the cursor.
    /// </summary>
    public void VerticalLine(Rune ch, int n)
    {
        if (ch.Value == 0) ch = Acs.VLine;
        for (int i = 0; i < n && (_cursorY + i) < MaxY; i++)
            _buffer[_cursorY + i, _cursorX] = new CharInfo(ch, _attrs);
    }

    // --- Scroll ---

    /// <summary>
    /// Enable or disable scrolling for this window.
    /// </summary>
    public void ScrollOk(bool enable) => _scrollOk = enable;

    /// <summary>
    /// Set the scrolling region (top and bottom rows, inclusive).
    /// </summary>
    public void SetScrollRegion(int top, int bottom)
    {
        if (top >= 0 && bottom < MaxY && top <= bottom)
        {
            _scrollTop = top;
            _scrollBottom = bottom;
        }
    }

    /// <summary>
    /// Scroll the window content by n lines (positive = up, negative = down).
    /// </summary>
    public void Scroll(int n)
    {
        if (n > 0)
        {
            // Scroll up
            for (int i = 0; i < n; i++)
                ScrollUp();
        }
        else if (n < 0)
        {
            // Scroll down
            for (int i = 0; i < -n; i++)
                ScrollDown();
        }
    }

    private void ScrollUp()
    {
        for (int r = _scrollTop; r < _scrollBottom; r++)
            for (int c = 0; c < MaxX; c++)
                _buffer[r, c] = _buffer[r + 1, c];

        for (int c = 0; c < MaxX; c++)
            _buffer[_scrollBottom, c] = CharInfo.Blank;
    }

    private void ScrollDown()
    {
        for (int r = _scrollBottom; r > _scrollTop; r--)
            for (int c = 0; c < MaxX; c++)
                _buffer[r, c] = _buffer[r - 1, c];

        for (int c = 0; c < MaxX; c++)
            _buffer[_scrollTop, c] = CharInfo.Blank;
    }

    // --- Input ---

    /// <summary>
    /// Enable or disable keypad mode for this window.
    /// </summary>
    public void Keypad(bool enable)
    {
        _keypadMode = enable;
        InputDriver.KeypadMode = enable;
    }

    /// <summary>
    /// Enable or disable non-blocking input for this window.
    /// </summary>
    public void NoDelay(bool enable) =>
        _timeoutMs = enable ? 0 : -1;

    /// <summary>
    /// Set input timeout in milliseconds (-1 = blocking, 0 = non-blocking).
    /// </summary>
    public void Timeout(int ms) => _timeoutMs = ms;

    /// <summary>
    /// Read a single key from input.
    /// </summary>
    public int GetChar()
    {
        // Apply this window's timeout setting
        int saved = InputDriver.TimeoutMs;
        bool savedKeypad = InputDriver.KeypadMode;
        InputDriver.TimeoutMs = _timeoutMs;
        InputDriver.KeypadMode = _keypadMode;

        int ch = InputDriver.GetChar();

        InputDriver.TimeoutMs = saved;
        InputDriver.KeypadMode = savedKeypad;
        return ch;
    }

    /// <summary>
    /// Read a string with basic line editing.
    /// </summary>
    public string GetString(int maxLen) => InputDriver.GetString(this, maxLen);

    // --- Refresh ---

    /// <summary>
    /// Copy this window's buffer to the virtual screen and update the terminal.
    /// Equivalent to NoOutRefresh() + DoUpdate().
    /// </summary>
    public void Refresh()
    {
        NoOutRefresh();
        NetCurses.DoUpdate();
    }

    /// <summary>
    /// Copy this window's buffer to the virtual screen without updating the terminal.
    /// </summary>
    public void NoOutRefresh()
    {
        NetCurses.GetScreen().CopyFromWindow(this);
    }

    // --- Window management ---

    /// <summary>
    /// Create a subwindow that shares the parent's buffer.
    /// </summary>
    public Window SubWindow(int rows, int cols, int beginY, int beginX)
    {
        var sub = new Window(rows, cols, beginY, beginX);
        sub._parent = this;
        return sub;
    }

    /// <summary>
    /// Move the window to a new position on the screen.
    /// </summary>
    public void MoveWindow(int y, int x)
    {
        BeginY = y;
        BeginX = x;
    }

    // --- Cursor advance helpers ---

    private void AdvanceCursor()
    {
        _cursorX++;
        if (_cursorX >= MaxX)
        {
            AdvanceCursorNewline();
        }
    }

    private void AdvanceCursorNewline()
    {
        _cursorX = 0;
        _cursorY++;
        if (_cursorY > _scrollBottom)
        {
            if (_scrollOk)
            {
                ScrollUp();
                _cursorY = _scrollBottom;
            }
            else
            {
                _cursorY = _scrollBottom;
            }
        }
    }
}
