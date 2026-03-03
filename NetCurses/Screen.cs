namespace BCyT.NetCurses;

/// <summary>
/// Virtual/physical screen diff engine.
/// Manages the two-buffer model: virtual screen (target) and physical screen (displayed).
/// DoUpdate() diffs them and emits minimal ANSI sequences.
/// </summary>
internal class Screen
{
    private CharInfo[,] _virtual;
    private CharInfo[,] _physical;
    private int _rows;
    private int _cols;
    private int _cursorRow;
    private int _cursorCol;

    public int Rows => _rows;
    public int Cols => _cols;

    public Screen(int rows, int cols)
    {
        _rows = rows;
        _cols = cols;
        _virtual = new CharInfo[rows, cols];
        _physical = new CharInfo[rows, cols];
        Clear();
        InvalidatePhysical();
    }

    /// <summary>
    /// Clear the virtual screen to blanks.
    /// </summary>
    public void Clear()
    {
        for (int r = 0; r < _rows; r++)
            for (int c = 0; c < _cols; c++)
                _virtual[r, c] = CharInfo.Blank;
    }

    /// <summary>
    /// Mark all physical cells as invalid so the next DoUpdate does a full redraw.
    /// </summary>
    private void InvalidatePhysical()
    {
        // Set physical to a sentinel value that won't match any real cell
        var sentinel = new CharInfo(new System.Text.Rune('\xFFFF'), uint.MaxValue);
        for (int r = 0; r < _rows; r++)
            for (int c = 0; c < _cols; c++)
                _physical[r, c] = sentinel;
    }

    /// <summary>
    /// Copy a window's buffer to the virtual screen at the window's position.
    /// </summary>
    public void CopyFromWindow(Window window)
    {
        int startRow = window.BeginY;
        int startCol = window.BeginX;
        int winRows = window.MaxY;
        int winCols = window.MaxX;

        for (int r = 0; r < winRows && (startRow + r) < _rows; r++)
        {
            for (int c = 0; c < winCols && (startCol + c) < _cols; c++)
            {
                _virtual[startRow + r, startCol + c] = window.GetCell(r, c);
            }
        }

        // Track cursor position from the most recently refreshed window
        _cursorRow = startRow + window.CursorY;
        _cursorCol = startCol + window.CursorX;
    }

    /// <summary>
    /// Diff virtual vs physical and emit minimal ANSI sequences.
    /// </summary>
    public void DoUpdate()
    {
        for (int r = 0; r < _rows; r++)
        {
            int c = 0;
            while (c < _cols)
            {
                // Skip unchanged cells
                if (_virtual[r, c] == _physical[r, c])
                {
                    c++;
                    continue;
                }

                // Found a changed cell - position cursor
                AnsiBackend.MoveCursor(r, c);

                // Batch contiguous changed cells on this row
                while (c < _cols && _virtual[r, c] != _physical[r, c])
                {
                    var cell = _virtual[r, c];
                    AnsiBackend.SetAttributes(cell.Attributes);
                    AnsiBackend.PutChar(cell.Character);
                    _physical[r, c] = cell;
                    c++;
                }
            }
        }

        // Position cursor at the window's cursor location
        AnsiBackend.MoveCursor(_cursorRow, _cursorCol);
        AnsiBackend.Flush();
    }

    /// <summary>
    /// Resize the screen buffers. Invalidates physical to force full redraw.
    /// </summary>
    public void Resize(int rows, int cols)
    {
        _rows = rows;
        _cols = cols;
        _virtual = new CharInfo[rows, cols];
        _physical = new CharInfo[rows, cols];
        Clear();
        InvalidatePhysical();
    }
}
