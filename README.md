# NetCurses

An ncurses implementation for .NET - build TUIs with C# and .NET 10.0

## Installation

```bash
dotnet add package BCyT.NetCurses
```

## Hello World

```csharp
using BCyT.NetCurses;

try
{
    Window window = NetCurses.InitScreen();
    NetCurses.MvAddString(NetCurses.Lines/2-1, NetCurses.Cols/2-5, "Hello World");
    NetCurses.Refresh();
    NetCurses.GetChar();
} finally
{
    NetCurses.EndWin();
}
```

## Documentation

See [TUTORIAL.md](TUTORIAL.md) for the full API walkthrough and [API_REFERENCE.html](API_REFERENCE.html) for a side-by-side comparison with the original C ncurses API.

## License

[ISC](LICENSE) - Copyright (c) 2026 César Bravo Molina
