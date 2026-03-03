namespace BCyT.NetCurses;

/// <summary>
/// Exception type for wcurses library errors.
/// </summary>
public class CursesException : Exception
{
    public CursesException() { }

    public CursesException(string message) : base(message) { }

    public CursesException(string message, Exception innerException)
        : base(message, innerException) { }
}
