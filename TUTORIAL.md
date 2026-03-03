# NetCurses Tutorial

NetCurses is an ncurses-compatible TUI (Text User Interface) framework for .NET. It lets you build terminal applications with windows, colors, text attributes, keyboard input, and box-drawing characters.

## Prerequisites

- **.NET 10.0** SDK or later
- A terminal that supports ANSI/VT escape sequences (all modern terminals do)

## Installation

Install the NuGet package:

```bash
dotnet add package BCyT.NetCurses
```

Then add the namespace to your source files:

```csharp
using wcurses;
```

---

## 1. Hello World

Every wcurses program follows the same lifecycle: **initialize**, **use**, **clean up**.

```csharp
using wcurses;

try
{
    Window stdscr = NetCurses.InitScreen();

    NetCurses.MvAddString(0, 0, "Hello, wcurses!");
    NetCurses.Refresh();
    NetCurses.GetChar(); // Wait for any key
}
finally
{
    NetCurses.EndWin(); // Always restore the terminal
}
```

Key points:
- `InitScreen()` enters the alternate screen buffer, hides the cursor, and returns the standard screen window (`stdscr`).
- `Refresh()` pushes your changes to the terminal.
- `EndWin()` restores the terminal to its original state. Always call it in a `finally` block so the terminal is cleaned up even if an exception occurs.

---

## 2. Writing Text

### Basic output

```csharp
// Move cursor to row 5, column 10 and write a string
NetCurses.MvAddString(5, 10, "Score: 100");

// Or move then write separately
NetCurses.Move(7, 10);
NetCurses.AddString("Health: 30/30");

// Printf-style formatting
NetCurses.MvPrint(9, 10, "Level {0} - Gold: {1}", level, gold);

// Single character
NetCurses.MvAddChar(3, 3, '@');
```

All coordinates are `(row, col)` -- row 0 is the top of the screen, column 0 is the left edge.

### Mv* convenience methods

Methods prefixed with `Mv` combine a `Move()` and an output operation in one call:

| Method | Equivalent to |
|--------|---------------|
| `MvAddChar(y, x, ch)` | `Move(y, x)` then `AddChar(ch)` |
| `MvAddString(y, x, str)` | `Move(y, x)` then `AddString(str)` |
| `MvPrint(y, x, fmt, ...)` | `Move(y, x)` then `Print(fmt, ...)` |

---

## 3. Text Attributes

Attributes control how text is styled. Apply them before writing text and they affect all subsequent output.

### Available attributes

| Constant | Effect |
|----------|--------|
| `Attrs.Normal` | Reset to default |
| `Attrs.Bold` | Bold / bright text |
| `Attrs.Dim` | Dim / faint text |
| `Attrs.Underline` | Underlined text |
| `Attrs.Italic` | Italic text |
| `Attrs.Reverse` | Swap foreground and background |
| `Attrs.Standout` | Reverse video (same as `Reverse`) |
| `Attrs.Blink` | Blinking text |
| `Attrs.Invisible` | Hidden text |

### Setting attributes

```csharp
// Turn on bold
NetCurses.AttributeOn(Attrs.Bold);
NetCurses.MvAddString(0, 0, "This is bold");

// Turn off bold
NetCurses.AttributeOff(Attrs.Bold);

// Set attributes to an exact value (replaces all current attributes)
NetCurses.AttributeSet(Attrs.Underline | Attrs.Italic);
NetCurses.MvAddString(1, 0, "Underline + italic");

// Reset to normal
NetCurses.AttributeSet(Attrs.Normal);
```

### Combining attributes with colors

Attributes can be OR'd together with color pairs (see the Colors section):

```csharp
NetCurses.AttributeSet(Attrs.Bold | Attrs.ColorPair(1));
NetCurses.MvAddString(0, 0, "Bold and colored!");
```

---

## 4. Colors

wcurses supports 8 standard ANSI colors and up to 255 user-defined color pairs.

### Setup

Colors must be initialized before use:

```csharp
NetCurses.StartColor();
```

### Defining color pairs

A color pair combines a foreground and background color. Pair numbers range from 1 to 255 (pair 0 is reserved as white-on-black).

```csharp
NetCurses.InitPair(1, Colors.Red,    Colors.Black);
NetCurses.InitPair(2, Colors.Green,  Colors.Black);
NetCurses.InitPair(3, Colors.Yellow, Colors.Blue);
NetCurses.InitPair(4, Colors.White,  Colors.Red);
```

### Available colors

| Constant | Value |
|----------|-------|
| `Colors.Black` | 0 |
| `Colors.Red` | 1 |
| `Colors.Green` | 2 |
| `Colors.Yellow` | 3 |
| `Colors.Blue` | 4 |
| `Colors.Magenta` | 5 |
| `Colors.Cyan` | 6 |
| `Colors.White` | 7 |

### Using color pairs

Apply a color pair as an attribute:

```csharp
// Method 1: AttributeSet
NetCurses.AttributeSet(Attrs.ColorPair(1));
NetCurses.MvAddString(0, 0, "Red text on black background");

// Method 2: combine with other attributes
NetCurses.AttributeSet(Attrs.Bold | Attrs.ColorPair(2));
NetCurses.MvAddString(1, 0, "Bold green text");

// Method 3: use NetCurses.ColorPair() shorthand
NetCurses.AttributeOn(NetCurses.ColorPair(3));
```

### Checking color support

```csharp
if (NetCurses.HasColors())
{
    NetCurses.StartColor();
    // set up pairs...
}
```

---

## 5. Input Handling

### Input modes

Configure how input is processed before entering your main loop:

```csharp
NetCurses.CBreak();    // Deliver keys immediately (no line buffering)
NetCurses.NoEcho();    // Don't echo typed characters to the screen
stdscr.Keypad(true); // Enable special key codes (arrows, F-keys, etc.)
```

| Mode | Method | Effect |
|------|--------|--------|
| CBreak | `NetCurses.CBreak()` / `NetCurses.NoCBreak()` | Characters available immediately without waiting for Enter |
| Raw | `NetCurses.Raw()` / `NetCurses.NoRaw()` | Like CBreak but also disables signal processing |
| Echo | `NetCurses.Echo()` / `NetCurses.NoEcho()` | Controls whether typed characters are shown on screen |
| Keypad | `window.Keypad(true/false)` | Enables extended key codes for arrows, function keys, etc. |

### Reading single keys

```csharp
int ch = NetCurses.GetChar(); // Blocks until a key is pressed

switch (ch)
{
    case Key.Up:    playerY--; break;
    case Key.Down:  playerY++; break;
    case Key.Left:  playerX--; break;
    case Key.Right: playerX++; break;
    case 'q':       running = false; break;
}
```

### Key constants

All key constants are in the `Key` class:

| Constant | Key |
|----------|-----|
| `Key.Up`, `Key.Down`, `Key.Left`, `Key.Right` | Arrow keys |
| `Key.Home`, `Key.End` | Home / End |
| `Key.PageUp`, `Key.PageDown` | Page Up / Page Down |
| `Key.Enter` | Enter |
| `Key.Backspace` | Backspace |
| `Key.Delete`, `Key.Insert` | Delete / Insert |
| `Key.F1` through `Key.F12` | Function keys |
| `Key.Resize` | Terminal was resized |
| `Key.ErrKey` | No key available (timeout) |

Regular printable characters are returned as their character value (e.g., `'a'` is 97).

### Reading strings

```csharp
NetCurses.MvAddString(5, 0, "Enter your name: ");
NetCurses.Refresh();
NetCurses.Echo(); // Turn echo on so the user can see what they type
string name = NetCurses.GetString(20); // Max 20 characters
NetCurses.NoEcho();
```

`GetString` supports Backspace for editing and Escape to cancel.

### Timeouts and non-blocking input

```csharp
// Non-blocking: GetChar() returns Key.ErrKey immediately if no key is pressed
stdscr.NoDelay(true);

// Timeout: wait up to 500ms for a key, then return Key.ErrKey
stdscr.Timeout(500);

// Blocking (default): wait forever
stdscr.Timeout(-1);

// Global half-delay: wait up to N tenths of a second
NetCurses.HalfDelay(5); // 0.5 seconds
```

### Pushing back input

```csharp
NetCurses.UngetChar('y'); // Next GetChar() will return 'y'
```

---

## 6. Windows

Windows are independent rectangular regions with their own cursor and character buffer.

### Creating windows

```csharp
// NewWindow(rows, cols, beginY, beginX)
Window infoPanel = NetCurses.NewWindow(10, 40, 0, 0);
Window logPanel  = NetCurses.NewWindow(10, 40, 0, 40);
```

### Writing to windows

Windows have the same API as the static `NetCurses` methods:

```csharp
infoPanel.MvAddString(1, 1, "Player Info");
infoPanel.MvPrint(3, 1, "HP: {0}/{1}", currentHp, maxHp);
infoPanel.MvPrint(4, 1, "Gold: {0}", gold);

logPanel.MvAddString(1, 1, "Message log");
logPanel.MvAddString(3, 1, "You enter the dungeon.");
```

### Refreshing windows

Each window must be refreshed to appear on screen:

```csharp
// Option 1: Refresh individually (updates terminal each time)
infoPanel.Refresh();
logPanel.Refresh();

// Option 2: Batch updates for efficiency (recommended with multiple windows)
infoPanel.NoOutRefresh();  // Copy to virtual screen
logPanel.NoOutRefresh();   // Copy to virtual screen
NetCurses.DoUpdate();        // Update terminal once
```

### Drawing borders

```csharp
// Box with default line-drawing characters
infoPanel.Box(Acs.VLine, Acs.HLine);

// Custom border (left, right, top, bottom, top-left, top-right, bottom-left, bottom-right)
logPanel.Border(Acs.VLine, Acs.VLine, Acs.HLine, Acs.HLine,
                Acs.ULCorner, Acs.URCorner, Acs.LLCorner, Acs.LRCorner);
```

### Lines

```csharp
// Draw a horizontal separator at row 5
infoPanel.Move(5, 1);
infoPanel.HorizontalLine(Acs.HLine, 38);

// Draw a vertical divider at column 20
infoPanel.Move(1, 20);
infoPanel.VerticalLine(Acs.VLine, 8);
```

### Window properties

```csharp
int rows = infoPanel.MaxY;     // Window height
int cols = infoPanel.MaxX;     // Window width
int posY = infoPanel.BeginY;   // Y position on screen
int posX = infoPanel.BeginX;   // X position on screen
int curY = infoPanel.CursorY;  // Current cursor row
int curX = infoPanel.CursorX;  // Current cursor column
```

### Moving and deleting windows

```csharp
NetCurses.MoveWindow(infoPanel, 5, 5); // Reposition on screen
NetCurses.DeleteWindow(infoPanel);       // Remove window
```

### Subwindows

Subwindows are children of a parent window:

```csharp
Window sub = infoPanel.SubWindow(5, 20, 2, 2);
// or via the static API:
Window sub2 = NetCurses.SubWindow(infoPanel, 5, 20, 2, 2);
```

---

## 7. Clearing the Screen

```csharp
NetCurses.Clear();           // Clear entire screen and move cursor to (0,0)
NetCurses.Erase();           // Clear entire screen without moving cursor
NetCurses.ClearToEndOfLine();// Clear from cursor to end of current line
NetCurses.ClearToBottom();   // Clear from cursor to bottom of screen
```

These also work on individual windows:

```csharp
infoPanel.Clear();
infoPanel.ClearToEndOfLine();
```

---

## 8. Box-Drawing Characters (ACS)

The `Acs` class provides Unicode box-drawing characters:

| Constant | Character | Description |
|----------|-----------|-------------|
| `Acs.ULCorner` | `┌` | Upper-left corner |
| `Acs.URCorner` | `┐` | Upper-right corner |
| `Acs.LLCorner` | `└` | Lower-left corner |
| `Acs.LRCorner` | `┘` | Lower-right corner |
| `Acs.LTee` | `├` | Left tee |
| `Acs.RTee` | `┤` | Right tee |
| `Acs.TTee` | `┬` | Top tee |
| `Acs.BTee` | `┴` | Bottom tee |
| `Acs.HLine` | `─` | Horizontal line |
| `Acs.VLine` | `│` | Vertical line |
| `Acs.Plus` | `┼` | Crossover / plus |
| `Acs.Diamond` | `◆` | Diamond |
| `Acs.Board` | `░` | Board pattern |
| `Acs.Block` | `█` | Solid block |
| `Acs.Bullet` | `·` | Bullet / middle dot |
| `Acs.Degree` | `°` | Degree sign |

Arrow symbols: `Acs.LArrow` (`<`), `Acs.RArrow` (`>`), `Acs.UArrow` (`^`), `Acs.DArrow` (`v`).

---

## 9. Scrolling

Enable scrolling on a window and optionally define a scroll region:

```csharp
Window logWin = NetCurses.NewWindow(10, 60, 12, 0);
logWin.ScrollOk(true);

// Optional: restrict scrolling to specific rows (0-indexed within the window)
logWin.SetScrollRegion(1, 8); // Only rows 1-8 scroll; row 0 and 9 stay fixed

// Scroll programmatically
logWin.Scroll(1);  // Scroll up 1 line
logWin.Scroll(-2); // Scroll down 2 lines
```

When `ScrollOk` is enabled, text that wraps past the last row automatically scrolls the window up.

---

## 10. Cursor Visibility

```csharp
NetCurses.CursorSet(0); // Invisible (good for games and menus)
NetCurses.CursorSet(1); // Normal
NetCurses.CursorSet(2); // Very visible
```

---

## 11. Terminal Information

```csharp
int rows = NetCurses.Lines; // Number of rows in the terminal
int cols = NetCurses.Cols;  // Number of columns in the terminal
```

---

## 12. Miscellaneous

```csharp
NetCurses.Beep();     // Sound the terminal bell
NetCurses.Flash();    // Visual bell (flash the screen)
NetCurses.Nap(1000);  // Sleep for 1000 milliseconds
```

---

## Complete Example: Interactive Status Display

```csharp
using wcurses;

try
{
    var stdscr = NetCurses.InitScreen();
    NetCurses.StartColor();
    NetCurses.CBreak();
    NetCurses.NoEcho();
    NetCurses.CursorSet(0);
    stdscr.Keypad(true);

    // Define color pairs
    NetCurses.InitPair(1, Colors.Green, Colors.Black);
    NetCurses.InitPair(2, Colors.Red,   Colors.Black);
    NetCurses.InitPair(3, Colors.Black, Colors.White);

    // Create a bordered window
    int rows = 12, cols = 40;
    int startY = (NetCurses.Lines - rows) / 2;
    int startX = (NetCurses.Cols - cols) / 2;
    Window win = NetCurses.NewWindow(rows, cols, startY, startX);

    int counter = 0;
    bool running = true;
    win.Timeout(100); // Non-blocking input with 100ms timeout

    while (running)
    {
        win.Erase();
        win.Box(Acs.VLine, Acs.HLine);

        // Title bar
        win.AttributeSet(Attrs.Bold | Attrs.ColorPair(3));
        win.MvAddString(0, 2, " Status Monitor ");
        win.AttributeSet(Attrs.Normal);

        // Content
        win.AttributeSet(Attrs.ColorPair(1));
        win.MvPrint(2, 2, "Counter: {0}", counter);

        win.AttributeSet(Attrs.Normal);
        win.MvPrint(4, 2, "Terminal: {0}x{1}", NetCurses.Cols, NetCurses.Lines);
        win.MvAddString(6, 2, "Press UP/DOWN to adjust");
        win.MvAddString(7, 2, "Press 'r' to reset");

        win.AttributeSet(Attrs.ColorPair(2) | Attrs.Dim);
        win.MvAddString(9, 2, "Press 'q' to quit");
        win.AttributeSet(Attrs.Normal);

        win.Refresh();

        int ch = win.GetChar();
        switch (ch)
        {
            case Key.Up:    counter++; break;
            case Key.Down:  counter--; break;
            case 'r':       counter = 0; break;
            case 'q':       running = false; break;
        }
    }

    NetCurses.DeleteWindow(win);
}
finally
{
    NetCurses.EndWin();
}
```

---

## Quick Reference

### Initialization

| Method | Description |
|--------|-------------|
| `NetCurses.InitScreen()` | Initialize and return stdscr |
| `NetCurses.EndWin()` | Restore terminal |
| `NetCurses.StartColor()` | Enable color support |

### Output

| Method | Description |
|--------|-------------|
| `AddChar(ch)` | Write a character at the cursor |
| `AddString(str)` | Write a string at the cursor |
| `Print(fmt, args)` | Printf-style formatted output |
| `MvAddChar(y, x, ch)` | Move then write character |
| `MvAddString(y, x, str)` | Move then write string |
| `MvPrint(y, x, fmt, args)` | Move then formatted output |
| `InsertChar(ch)` | Insert character, shifting line right |
| `DeleteChar()` | Delete character, shifting line left |

### Attributes & Colors

| Method | Description |
|--------|-------------|
| `AttributeOn(attrs)` | Enable attribute flags |
| `AttributeOff(attrs)` | Disable attribute flags |
| `AttributeSet(attrs)` | Set attributes exactly |
| `InitPair(n, fg, bg)` | Define color pair |
| `Attrs.ColorPair(n)` | Get attribute value for a color pair |

### Input

| Method | Description |
|--------|-------------|
| `GetChar()` | Read a single key (blocking by default) |
| `GetString(maxLen)` | Read a string with line editing |
| `UngetChar(ch)` | Push a character back into the input buffer |
| `CBreak()` / `NoCBreak()` | Toggle character-at-a-time input |
| `Raw()` / `NoRaw()` | Toggle raw input mode |
| `Echo()` / `NoEcho()` | Toggle input echo |

### Windows

| Method | Description |
|--------|-------------|
| `NewWindow(rows, cols, y, x)` | Create a window |
| `DeleteWindow(win)` | Delete a window |
| `win.Refresh()` | Refresh a single window |
| `win.NoOutRefresh()` | Stage refresh (call `DoUpdate()` after) |
| `win.Box(vert, horiz)` | Draw a border |
| `win.Timeout(ms)` | Set input timeout |
| `win.Keypad(enable)` | Enable special key codes |
