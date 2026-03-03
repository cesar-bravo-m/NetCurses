using System.Text;

namespace BCyT.NetCurses.Tests;

public class AcsTests
{
    [Test]
    public void BoxDrawingCorners_AreCorrectUnicode()
    {
        Assert.That(Acs.ULCorner, Is.EqualTo(new Rune('┌')));
        Assert.That(Acs.URCorner, Is.EqualTo(new Rune('┐')));
        Assert.That(Acs.LLCorner, Is.EqualTo(new Rune('└')));
        Assert.That(Acs.LRCorner, Is.EqualTo(new Rune('┘')));
    }

    [Test]
    public void BoxDrawingTees_AreCorrectUnicode()
    {
        Assert.That(Acs.LTee, Is.EqualTo(new Rune('├')));
        Assert.That(Acs.RTee, Is.EqualTo(new Rune('┤')));
        Assert.That(Acs.TTee, Is.EqualTo(new Rune('┬')));
        Assert.That(Acs.BTee, Is.EqualTo(new Rune('┴')));
    }

    [Test]
    public void BoxDrawingLines_AreCorrectUnicode()
    {
        Assert.That(Acs.HLine, Is.EqualTo(new Rune('─')));
        Assert.That(Acs.VLine, Is.EqualTo(new Rune('│')));
        Assert.That(Acs.Plus, Is.EqualTo(new Rune('┼')));
    }

    [Test]
    public void SpecialCharacters_AreCorrectUnicode()
    {
        Assert.That(Acs.Diamond, Is.EqualTo(new Rune('◆')));
        Assert.That(Acs.Board, Is.EqualTo(new Rune('░')));
        Assert.That(Acs.Block, Is.EqualTo(new Rune('█')));
        Assert.That(Acs.Bullet, Is.EqualTo(new Rune('·')));
        Assert.That(Acs.Degree, Is.EqualTo(new Rune('°')));
    }

    [Test]
    public void ArrowCharacters_AreCorrectAscii()
    {
        Assert.That(Acs.LArrow, Is.EqualTo(new Rune('<')));
        Assert.That(Acs.RArrow, Is.EqualTo(new Rune('>')));
        Assert.That(Acs.UArrow, Is.EqualTo(new Rune('^')));
        Assert.That(Acs.DArrow, Is.EqualTo(new Rune('v')));
    }

    [Test]
    public void AllConstants_AreNonDefault()
    {
        Rune defaultRune = default;

        Assert.That(Acs.ULCorner, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.URCorner, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.LLCorner, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.LRCorner, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.LTee, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.RTee, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.TTee, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.BTee, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.HLine, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.VLine, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.Plus, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.Diamond, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.Board, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.Block, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.Bullet, Is.Not.EqualTo(defaultRune));
        Assert.That(Acs.Degree, Is.Not.EqualTo(defaultRune));
    }
}
