namespace BCyT.NetCurses.Tests;

public class ColorsTests
{
    [SetUp]
    public void Setup()
    {
        Colors.Reset();
    }

    [Test]
    public void ColorConstants_HaveExpectedValues()
    {
        Assert.That(Colors.Black, Is.EqualTo((short)0));
        Assert.That(Colors.Red, Is.EqualTo((short)1));
        Assert.That(Colors.Green, Is.EqualTo((short)2));
        Assert.That(Colors.Yellow, Is.EqualTo((short)3));
        Assert.That(Colors.Blue, Is.EqualTo((short)4));
        Assert.That(Colors.Magenta, Is.EqualTo((short)5));
        Assert.That(Colors.Cyan, Is.EqualTo((short)6));
        Assert.That(Colors.White, Is.EqualTo((short)7));
    }

    [Test]
    public void HasColors_ReturnsTrue()
    {
        Assert.That(Colors.HasColors(), Is.True);
    }

    [Test]
    public void StartColor_InitializesPairZero()
    {
        Colors.StartColor();

        var pair0 = Colors.GetPair(0);
        Assert.That(pair0.Fg, Is.EqualTo(Colors.White));
        Assert.That(pair0.Bg, Is.EqualTo(Colors.Black));
    }

    [Test]
    public void InitPair_WithoutStartColor_Throws()
    {
        Assert.Throws<CursesException>(() => Colors.InitPair(1, Colors.Red, Colors.Black));
    }

    [Test]
    public void InitPair_StoresColorPair()
    {
        Colors.StartColor();
        Colors.InitPair(1, Colors.Green, Colors.Blue);

        var pair = Colors.GetPair(1);
        Assert.That(pair.Fg, Is.EqualTo(Colors.Green));
        Assert.That(pair.Bg, Is.EqualTo(Colors.Blue));
    }

    [Test]
    public void InitPair_PairZero_Throws()
    {
        Colors.StartColor();

        Assert.Throws<CursesException>(() => Colors.InitPair(0, Colors.Red, Colors.Black));
    }

    [Test]
    public void InitPair_NegativePair_Throws()
    {
        Colors.StartColor();

        Assert.Throws<CursesException>(() => Colors.InitPair(-1, Colors.Red, Colors.Black));
    }

    [Test]
    public void InitPair_PairTooLarge_Throws()
    {
        Colors.StartColor();

        Assert.Throws<CursesException>(() => Colors.InitPair(256, Colors.Red, Colors.Black));
    }

    [Test]
    public void InitPair_MaxValidPair_Succeeds()
    {
        Colors.StartColor();
        Colors.InitPair(255, Colors.Cyan, Colors.Magenta);

        var pair = Colors.GetPair(255);
        Assert.That(pair.Fg, Is.EqualTo(Colors.Cyan));
        Assert.That(pair.Bg, Is.EqualTo(Colors.Magenta));
    }

    [Test]
    public void InitPair_MinValidPair_Succeeds()
    {
        Colors.StartColor();
        Colors.InitPair(1, Colors.Yellow, Colors.Red);

        var pair = Colors.GetPair(1);
        Assert.That(pair.Fg, Is.EqualTo(Colors.Yellow));
        Assert.That(pair.Bg, Is.EqualTo(Colors.Red));
    }

    [Test]
    public void InitPair_CanOverwriteExistingPair()
    {
        Colors.StartColor();
        Colors.InitPair(1, Colors.Red, Colors.Black);
        Colors.InitPair(1, Colors.Green, Colors.White);

        var pair = Colors.GetPair(1);
        Assert.That(pair.Fg, Is.EqualTo(Colors.Green));
        Assert.That(pair.Bg, Is.EqualTo(Colors.White));
    }

    [Test]
    public void GetPair_OutOfRange_ReturnsWhiteOnBlack()
    {
        Colors.StartColor();

        var pair = Colors.GetPair(-1);
        Assert.That(pair.Fg, Is.EqualTo(Colors.White));
        Assert.That(pair.Bg, Is.EqualTo(Colors.Black));

        pair = Colors.GetPair(256);
        Assert.That(pair.Fg, Is.EqualTo(Colors.White));
        Assert.That(pair.Bg, Is.EqualTo(Colors.Black));
    }

    [Test]
    public void Reset_ClearsState()
    {
        Colors.StartColor();
        Colors.InitPair(1, Colors.Red, Colors.Blue);
        Colors.Reset();

        // After reset, InitPair should fail (StartColor not called)
        Assert.Throws<CursesException>(() => Colors.InitPair(2, Colors.Red, Colors.Black));
    }

    [Test]
    public void MultiplePairs_IndependentlyStored()
    {
        Colors.StartColor();
        Colors.InitPair(1, Colors.Red, Colors.Black);
        Colors.InitPair(2, Colors.Green, Colors.Blue);
        Colors.InitPair(3, Colors.Yellow, Colors.Cyan);

        Assert.That(Colors.GetPair(1).Fg, Is.EqualTo(Colors.Red));
        Assert.That(Colors.GetPair(2).Fg, Is.EqualTo(Colors.Green));
        Assert.That(Colors.GetPair(3).Fg, Is.EqualTo(Colors.Yellow));
    }
}
