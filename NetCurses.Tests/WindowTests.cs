using System.Text;

namespace BCyT.NetCurses.Tests;

public class WindowTests
{
    private Window _win = null!;

    [SetUp]
    public void Setup()
    {
        _win = new Window(10, 20, 0, 0);
    }

    // --- Dimensions ---

    [Test]
    public void Constructor_SetsDimensions()
    {
        Assert.That(_win.MaxY, Is.EqualTo(10));
        Assert.That(_win.MaxX, Is.EqualTo(20));
        Assert.That(_win.BeginY, Is.EqualTo(0));
        Assert.That(_win.BeginX, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithOffset_SetsPosition()
    {
        var win = new Window(5, 10, 3, 7);

        Assert.That(win.BeginY, Is.EqualTo(3));
        Assert.That(win.BeginX, Is.EqualTo(7));
    }

    [Test]
    public void Constructor_InitializesCursorAtOrigin()
    {
        Assert.That(_win.CursorY, Is.EqualTo(0));
        Assert.That(_win.CursorX, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_InitializesBufferWithBlanks()
    {
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 20; c++)
                Assert.That(_win.GetCell(r, c), Is.EqualTo(CharInfo.Blank));
    }

    // --- Cursor Movement ---

    [Test]
    public void Move_SetsCursorPosition()
    {
        _win.Move(3, 5);

        Assert.That(_win.CursorY, Is.EqualTo(3));
        Assert.That(_win.CursorX, Is.EqualTo(5));
    }

    [Test]
    public void Move_OutOfBounds_Ignored()
    {
        _win.Move(3, 5);
        _win.Move(-1, 0);

        Assert.That(_win.CursorY, Is.EqualTo(3));
        Assert.That(_win.CursorX, Is.EqualTo(5));
    }

    [Test]
    public void Move_AtBoundary_Ignored()
    {
        _win.Move(3, 5);
        _win.Move(10, 20); // MaxY=10, MaxX=20 are out of bounds

        Assert.That(_win.CursorY, Is.EqualTo(3));
        Assert.That(_win.CursorX, Is.EqualTo(5));
    }

    [Test]
    public void Move_LastValidPosition()
    {
        _win.Move(9, 19);

        Assert.That(_win.CursorY, Is.EqualTo(9));
        Assert.That(_win.CursorX, Is.EqualTo(19));
    }

    [Test]
    public void GetYX_ReturnsCursorPosition()
    {
        _win.Move(4, 8);
        _win.GetYX(out int y, out int x);

        Assert.That(y, Is.EqualTo(4));
        Assert.That(x, Is.EqualTo(8));
    }

    // --- Attributes ---

    [Test]
    public void AttributeOn_EnablesFlags()
    {
        _win.AttributeOn(Attrs.Bold);
        _win.AddChar(new Rune('A'));

        Assert.That(_win.GetCell(0, 0).Attributes & Attrs.Bold, Is.Not.EqualTo(0u));
    }

    [Test]
    public void AttributeOff_DisablesFlags()
    {
        _win.AttributeOn(Attrs.Bold | Attrs.Underline);
        _win.AttributeOff(Attrs.Bold);
        _win.AddChar(new Rune('A'));

        Assert.That(_win.GetCell(0, 0).Attributes & Attrs.Bold, Is.EqualTo(0u));
        Assert.That(_win.GetCell(0, 0).Attributes & Attrs.Underline, Is.Not.EqualTo(0u));
    }

    [Test]
    public void AttributeSet_ReplacesAllAttributes()
    {
        _win.AttributeOn(Attrs.Bold | Attrs.Underline);
        _win.AttributeSet(Attrs.Italic);
        _win.AddChar(new Rune('A'));

        Assert.That(_win.GetCell(0, 0).Attributes, Is.EqualTo(Attrs.Italic));
    }

    [Test]
    public void Standout_EnablesStandoutFlag()
    {
        _win.Standout();
        _win.AddChar(new Rune('A'));

        Assert.That(_win.GetCell(0, 0).Attributes & Attrs.Standout, Is.Not.EqualTo(0u));
    }

    [Test]
    public void StandEnd_DisablesStandoutFlag()
    {
        _win.Standout();
        _win.StandEnd();
        _win.AddChar(new Rune('A'));

        Assert.That(_win.GetCell(0, 0).Attributes & Attrs.Standout, Is.EqualTo(0u));
    }

    // --- Character Output ---

    [Test]
    public void AddChar_WritesToBuffer()
    {
        _win.AddChar(new Rune('X'));

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('X')));
    }

    [Test]
    public void AddChar_AdvancesCursor()
    {
        _win.AddChar(new Rune('A'));

        Assert.That(_win.CursorX, Is.EqualTo(1));
        Assert.That(_win.CursorY, Is.EqualTo(0));
    }

    [Test]
    public void AddChar_IntOverload_WritesToBuffer()
    {
        _win.AddChar('Z');

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('Z')));
    }

    [Test]
    public void AddChar_WithExplicitAttrs_UsesThoseAttrs()
    {
        _win.AttributeSet(Attrs.Bold);
        _win.AddChar(new Rune('A'), Attrs.Italic);

        Assert.That(_win.GetCell(0, 0).Attributes, Is.EqualTo(Attrs.Italic));
    }

    [Test]
    public void AddChar_Newline_ClearsToEndAndAdvancesLine()
    {
        _win.AddChar(new Rune('A'));
        _win.AddChar(new Rune('B'));
        _win.Move(0, 0);
        _win.AddChar(new Rune('\n'));

        Assert.That(_win.CursorY, Is.EqualTo(1));
        Assert.That(_win.CursorX, Is.EqualTo(0));
    }

    [Test]
    public void AddChar_Tab_ExpandsToSpaces()
    {
        _win.AddChar(new Rune('\t'));

        // Tab at column 0 should expand to 8 spaces
        Assert.That(_win.CursorX, Is.EqualTo(8));
        for (int i = 0; i < 8; i++)
            Assert.That(_win.GetCell(0, i).Character, Is.EqualTo(new Rune(' ')));
    }

    [Test]
    public void AddChar_Tab_AlignsToTabStop()
    {
        _win.AddChar(new Rune('A'));
        _win.AddChar(new Rune('B'));
        _win.AddChar(new Rune('\t'));

        // At column 2, tab should go to column 8 (6 spaces)
        Assert.That(_win.CursorX, Is.EqualTo(8));
    }

    [Test]
    public void AddChar_WrapsToNextLine()
    {
        for (int i = 0; i < 20; i++)
            _win.AddChar(new Rune('A'));

        Assert.That(_win.CursorY, Is.EqualTo(1));
        Assert.That(_win.CursorX, Is.EqualTo(0));
    }

    [Test]
    public void AddString_WritesMultipleChars()
    {
        _win.AddString("Hello");

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('H')));
        Assert.That(_win.GetCell(0, 1).Character, Is.EqualTo(new Rune('e')));
        Assert.That(_win.GetCell(0, 2).Character, Is.EqualTo(new Rune('l')));
        Assert.That(_win.GetCell(0, 3).Character, Is.EqualTo(new Rune('l')));
        Assert.That(_win.GetCell(0, 4).Character, Is.EqualTo(new Rune('o')));
        Assert.That(_win.CursorX, Is.EqualTo(5));
    }

    [Test]
    public void Print_FormatsAndWritesString()
    {
        _win.Print("x={0}", 42);

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('x')));
        Assert.That(_win.GetCell(0, 1).Character, Is.EqualTo(new Rune('=')));
        Assert.That(_win.GetCell(0, 2).Character, Is.EqualTo(new Rune('4')));
        Assert.That(_win.GetCell(0, 3).Character, Is.EqualTo(new Rune('2')));
    }

    // --- Mv* Variants ---

    [Test]
    public void MvAddChar_MovesAndWritesChar()
    {
        _win.MvAddChar(2, 3, new Rune('Q'));

        Assert.That(_win.GetCell(2, 3).Character, Is.EqualTo(new Rune('Q')));
    }

    [Test]
    public void MvAddChar_IntOverload_MovesAndWritesChar()
    {
        _win.MvAddChar(1, 1, 'Z');

        Assert.That(_win.GetCell(1, 1).Character, Is.EqualTo(new Rune('Z')));
    }

    [Test]
    public void MvAddString_MovesAndWritesString()
    {
        _win.MvAddString(3, 2, "Hi");

        Assert.That(_win.GetCell(3, 2).Character, Is.EqualTo(new Rune('H')));
        Assert.That(_win.GetCell(3, 3).Character, Is.EqualTo(new Rune('i')));
    }

    [Test]
    public void MvPrint_MovesAndWritesFormattedString()
    {
        _win.MvPrint(1, 0, "{0}!", "OK");

        Assert.That(_win.GetCell(1, 0).Character, Is.EqualTo(new Rune('O')));
        Assert.That(_win.GetCell(1, 1).Character, Is.EqualTo(new Rune('K')));
        Assert.That(_win.GetCell(1, 2).Character, Is.EqualTo(new Rune('!')));
    }

    // --- Insert / Delete Char ---

    [Test]
    public void InsertChar_ShiftsLineRight()
    {
        _win.AddString("BCD");
        _win.Move(0, 0);
        _win.InsertChar(new Rune('A'));

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('A')));
        Assert.That(_win.GetCell(0, 1).Character, Is.EqualTo(new Rune('B')));
        Assert.That(_win.GetCell(0, 2).Character, Is.EqualTo(new Rune('C')));
        Assert.That(_win.GetCell(0, 3).Character, Is.EqualTo(new Rune('D')));
    }

    [Test]
    public void InsertChar_DoesNotMoveCursor()
    {
        _win.Move(0, 2);
        _win.InsertChar(new Rune('X'));

        Assert.That(_win.CursorX, Is.EqualTo(2));
    }

    [Test]
    public void DeleteChar_ShiftsLineLeft()
    {
        _win.AddString("ABCD");
        _win.Move(0, 1);
        _win.DeleteChar();

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('A')));
        Assert.That(_win.GetCell(0, 1).Character, Is.EqualTo(new Rune('C')));
        Assert.That(_win.GetCell(0, 2).Character, Is.EqualTo(new Rune('D')));
        Assert.That(_win.GetCell(0, 3), Is.EqualTo(CharInfo.Blank));
    }

    [Test]
    public void DeleteChar_DoesNotMoveCursor()
    {
        _win.AddString("ABC");
        _win.Move(0, 1);
        _win.DeleteChar();

        Assert.That(_win.CursorX, Is.EqualTo(1));
    }

    // --- Clear ---

    [Test]
    public void Clear_FillsWithBlanksAndResetsCursor()
    {
        _win.AddString("Hello");
        _win.Clear();

        Assert.That(_win.CursorY, Is.EqualTo(0));
        Assert.That(_win.CursorX, Is.EqualTo(0));
        for (int c = 0; c < 5; c++)
            Assert.That(_win.GetCell(0, c), Is.EqualTo(CharInfo.Blank));
    }

    [Test]
    public void Erase_FillsWithBlanks_KeepsCursor()
    {
        _win.AddString("Hello");
        _win.Move(2, 3);
        _win.Erase();

        Assert.That(_win.CursorY, Is.EqualTo(2));
        Assert.That(_win.CursorX, Is.EqualTo(3));
        for (int c = 0; c < 5; c++)
            Assert.That(_win.GetCell(0, c), Is.EqualTo(CharInfo.Blank));
    }

    [Test]
    public void ClearToEndOfLine_ClearsFromCursorToEnd()
    {
        _win.AddString("Hello World!");
        _win.Move(0, 5);
        _win.ClearToEndOfLine();

        // "Hello" should remain
        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('H')));
        Assert.That(_win.GetCell(0, 4).Character, Is.EqualTo(new Rune('o')));
        // Rest should be blank
        for (int c = 5; c < 20; c++)
            Assert.That(_win.GetCell(0, c), Is.EqualTo(CharInfo.Blank));
    }

    [Test]
    public void ClearToBottom_ClearsFromCursorDown()
    {
        _win.MvAddString(0, 0, "Line 0");
        _win.MvAddString(1, 0, "Line 1");
        _win.MvAddString(2, 0, "Line 2");
        _win.Move(1, 3);
        _win.ClearToBottom();

        // Line 0 should be intact
        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('L')));
        // Line 1 partial: "Lin" should remain, rest cleared
        Assert.That(_win.GetCell(1, 0).Character, Is.EqualTo(new Rune('L')));
        Assert.That(_win.GetCell(1, 2).Character, Is.EqualTo(new Rune('n')));
        Assert.That(_win.GetCell(1, 3), Is.EqualTo(CharInfo.Blank));
        // Line 2 should be fully cleared
        for (int c = 0; c < 20; c++)
            Assert.That(_win.GetCell(2, c), Is.EqualTo(CharInfo.Blank));
    }

    // --- Border / Box ---

    [Test]
    public void Box_DrawsDefaultBorder()
    {
        var win = new Window(5, 10, 0, 0);
        win.Box(new Rune(0), new Rune(0));

        // Corners should use ACS defaults
        Assert.That(win.GetCell(0, 0).Character, Is.EqualTo(Acs.ULCorner));
        Assert.That(win.GetCell(0, 9).Character, Is.EqualTo(Acs.URCorner));
        Assert.That(win.GetCell(4, 0).Character, Is.EqualTo(Acs.LLCorner));
        Assert.That(win.GetCell(4, 9).Character, Is.EqualTo(Acs.LRCorner));

        // Horizontal lines
        for (int c = 1; c < 9; c++)
        {
            Assert.That(win.GetCell(0, c).Character, Is.EqualTo(Acs.HLine));
            Assert.That(win.GetCell(4, c).Character, Is.EqualTo(Acs.HLine));
        }

        // Vertical lines
        for (int r = 1; r < 4; r++)
        {
            Assert.That(win.GetCell(r, 0).Character, Is.EqualTo(Acs.VLine));
            Assert.That(win.GetCell(r, 9).Character, Is.EqualTo(Acs.VLine));
        }
    }

    [Test]
    public void Border_CustomCharacters_UsesProvided()
    {
        var win = new Window(3, 3, 0, 0);
        win.Border(
            new Rune('|'), new Rune('|'),
            new Rune('-'), new Rune('-'),
            new Rune('+'), new Rune('+'),
            new Rune('+'), new Rune('+'));

        Assert.That(win.GetCell(0, 0).Character, Is.EqualTo(new Rune('+')));
        Assert.That(win.GetCell(0, 1).Character, Is.EqualTo(new Rune('-')));
        Assert.That(win.GetCell(1, 0).Character, Is.EqualTo(new Rune('|')));
    }

    [Test]
    public void HorizontalLine_DrawsFromCursor()
    {
        _win.Move(2, 1);
        _win.HorizontalLine(Acs.HLine, 5);

        for (int c = 1; c < 6; c++)
            Assert.That(_win.GetCell(2, c).Character, Is.EqualTo(Acs.HLine));

        // Should not go beyond requested length
        Assert.That(_win.GetCell(2, 6), Is.EqualTo(CharInfo.Blank));
    }

    [Test]
    public void HorizontalLine_ClampedToWindowWidth()
    {
        _win.Move(0, 18);
        _win.HorizontalLine(Acs.HLine, 10);

        Assert.That(_win.GetCell(0, 18).Character, Is.EqualTo(Acs.HLine));
        Assert.That(_win.GetCell(0, 19).Character, Is.EqualTo(Acs.HLine));
        // No crash, no out-of-bounds
    }

    [Test]
    public void HorizontalLine_ZeroChar_UsesDefault()
    {
        _win.Move(0, 0);
        _win.HorizontalLine(new Rune(0), 3);

        for (int c = 0; c < 3; c++)
            Assert.That(_win.GetCell(0, c).Character, Is.EqualTo(Acs.HLine));
    }

    [Test]
    public void VerticalLine_DrawsFromCursor()
    {
        _win.Move(1, 3);
        _win.VerticalLine(Acs.VLine, 4);

        for (int r = 1; r < 5; r++)
            Assert.That(_win.GetCell(r, 3).Character, Is.EqualTo(Acs.VLine));

        Assert.That(_win.GetCell(5, 3), Is.EqualTo(CharInfo.Blank));
    }

    [Test]
    public void VerticalLine_ClampedToWindowHeight()
    {
        _win.Move(8, 0);
        _win.VerticalLine(Acs.VLine, 10);

        Assert.That(_win.GetCell(8, 0).Character, Is.EqualTo(Acs.VLine));
        Assert.That(_win.GetCell(9, 0).Character, Is.EqualTo(Acs.VLine));
    }

    [Test]
    public void VerticalLine_ZeroChar_UsesDefault()
    {
        _win.Move(0, 0);
        _win.VerticalLine(new Rune(0), 3);

        for (int r = 0; r < 3; r++)
            Assert.That(_win.GetCell(r, 0).Character, Is.EqualTo(Acs.VLine));
    }

    // --- Scrolling ---

    [Test]
    public void ScrollUp_ShiftsContentUp()
    {
        _win.MvAddString(0, 0, "Line0");
        _win.MvAddString(1, 0, "Line1");
        _win.MvAddString(2, 0, "Line2");
        _win.Scroll(1);

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('L')));
        Assert.That(_win.GetCell(0, 4).Character, Is.EqualTo(new Rune('1')));
        Assert.That(_win.GetCell(1, 4).Character, Is.EqualTo(new Rune('2')));
        // Last line should be blank
        Assert.That(_win.GetCell(9, 0), Is.EqualTo(CharInfo.Blank));
    }

    [Test]
    public void ScrollDown_ShiftsContentDown()
    {
        _win.MvAddString(0, 0, "Line0");
        _win.MvAddString(1, 0, "Line1");
        _win.Scroll(-1);

        // First line should be blank
        Assert.That(_win.GetCell(0, 0), Is.EqualTo(CharInfo.Blank));
        // Original line 0 content should be at line 1
        Assert.That(_win.GetCell(1, 0).Character, Is.EqualTo(new Rune('L')));
        Assert.That(_win.GetCell(1, 4).Character, Is.EqualTo(new Rune('0')));
    }

    [Test]
    public void Scroll_Zero_NoEffect()
    {
        _win.MvAddString(0, 0, "Test");
        _win.Scroll(0);

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('T')));
    }

    [Test]
    public void SetScrollRegion_LimitsScrollArea()
    {
        _win.MvAddString(0, 0, "Keep");
        _win.MvAddString(1, 0, "Scroll1");
        _win.MvAddString(2, 0, "Scroll2");
        _win.MvAddString(3, 0, "Scroll3");
        _win.MvAddString(4, 0, "Keep2");

        _win.SetScrollRegion(1, 3);
        _win.Scroll(1);

        // Line 0 should be unchanged
        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('K')));
        // Lines 1-3 should have scrolled up: line 1 now has "Scroll2"
        Assert.That(_win.GetCell(1, 6).Character, Is.EqualTo(new Rune('2')));
        // Line 2 now has "Scroll3"
        Assert.That(_win.GetCell(2, 6).Character, Is.EqualTo(new Rune('3')));
        // Line 3 should now be blank (within scroll region)
        Assert.That(_win.GetCell(3, 0), Is.EqualTo(CharInfo.Blank));
        // Line 4 should be unchanged
        Assert.That(_win.GetCell(4, 0).Character, Is.EqualTo(new Rune('K')));
    }

    [Test]
    public void ScrollOk_AutoScrollsOnLineWrap()
    {
        var win = new Window(3, 5, 0, 0);
        win.ScrollOk(true);

        win.MvAddString(0, 0, "AAAAA");
        win.MvAddString(1, 0, "BBBBB");
        win.MvAddString(2, 0, "CCCCC");

        // This should cause a scroll since we're past the last line
        // Writing at end of last line wraps and triggers scroll
        Assert.That(win.CursorY, Is.EqualTo(2));
    }

    // --- Window Management ---

    [Test]
    public void MoveWindow_ChangesPosition()
    {
        _win.MoveWindow(5, 10);

        Assert.That(_win.BeginY, Is.EqualTo(5));
        Assert.That(_win.BeginX, Is.EqualTo(10));
    }

    [Test]
    public void SubWindow_CreatesChildWindow()
    {
        var sub = _win.SubWindow(5, 10, 2, 3);

        Assert.That(sub.MaxY, Is.EqualTo(5));
        Assert.That(sub.MaxX, Is.EqualTo(10));
        Assert.That(sub.BeginY, Is.EqualTo(2));
        Assert.That(sub.BeginX, Is.EqualTo(3));
    }

    // --- NoDelay / Timeout ---

    [Test]
    public void NoDelay_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _win.NoDelay(true));
        Assert.DoesNotThrow(() => _win.NoDelay(false));
    }

    [Test]
    public void Timeout_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _win.Timeout(100));
        Assert.DoesNotThrow(() => _win.Timeout(-1));
        Assert.DoesNotThrow(() => _win.Timeout(0));
    }

    // --- Unicode ---

    [Test]
    public void AddChar_Unicode_PreservedInBuffer()
    {
        _win.AddChar(new Rune('★'));

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('★')));
    }

    [Test]
    public void AddString_Unicode_PreservedInBuffer()
    {
        _win.AddString("αβγ");

        Assert.That(_win.GetCell(0, 0).Character, Is.EqualTo(new Rune('α')));
        Assert.That(_win.GetCell(0, 1).Character, Is.EqualTo(new Rune('β')));
        Assert.That(_win.GetCell(0, 2).Character, Is.EqualTo(new Rune('γ')));
    }
}
