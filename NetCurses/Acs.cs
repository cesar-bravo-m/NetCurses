using System.Text;

namespace BCyT.NetCurses;

/// <summary>
/// ACS (Alternate Character Set) line-drawing characters using Unicode box-drawing.
/// </summary>
public static class Acs
{
    public static readonly Rune ULCorner = new('┌');
    public static readonly Rune URCorner = new('┐');
    public static readonly Rune LLCorner = new('└');
    public static readonly Rune LRCorner = new('┘');
    public static readonly Rune LTee     = new('├');
    public static readonly Rune RTee     = new('┤');
    public static readonly Rune TTee     = new('┬');
    public static readonly Rune BTee     = new('┴');
    public static readonly Rune HLine    = new('─');
    public static readonly Rune VLine    = new('│');
    public static readonly Rune Plus     = new('┼');
    public static readonly Rune Diamond  = new('◆');
    public static readonly Rune Board    = new('░');
    public static readonly Rune Block    = new('█');
    public static readonly Rune Bullet   = new('·');
    public static readonly Rune Degree   = new('°');
    public static readonly Rune LArrow   = new('<');
    public static readonly Rune RArrow   = new('>');
    public static readonly Rune UArrow   = new('^');
    public static readonly Rune DArrow   = new('v');
}
