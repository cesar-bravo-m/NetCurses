# NetCurses

An ncurses implementation for .NET - build TUIs with C# and .NET 10.0

<img width="1105" height="687" alt="image" src="https://github.com/user-attachments/assets/751a3458-889b-4f81-92f0-5a5608a534cc" />

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

<img width="1726" height="916" alt="image" src="https://github.com/user-attachments/assets/44b38a35-9ed4-4717-b6dc-3e0a7bf42ad1" />

## Documentation

See [TUTORIAL.md](TUTORIAL.md) for the full API walkthrough and [API_REFERENCE.html](API_REFERENCE.html) for a side-by-side comparison with the original C ncurses API.

## License

[ISC](LICENSE) - Copyright (c) 2026 César Bravo Molina
