using BCyT.NetCurses;

try
{
    var msg = "Hello world";
    Window window = NetCurses.InitScreen();
    NetCurses.AttributeOn(Attrs.Bold | Attrs.Italic);
    NetCurses.MvAddString(NetCurses.Lines/2 - 1, (NetCurses.Cols - msg.Length)/2, msg);
    NetCurses.Refresh();
    NetCurses.GetChar();
} finally
{
    NetCurses.EndWin();
}