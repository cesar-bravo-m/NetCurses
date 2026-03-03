namespace BCyT.NetCurses.Tests;

public class KeyTests
{
    [Test]
    public void ErrKey_IsNegativeOne()
    {
        Assert.That(Key.ErrKey, Is.EqualTo(-1));
    }

    [Test]
    public void ArrowKeys_AreDistinct()
    {
        int[] arrows = [Key.Up, Key.Down, Key.Left, Key.Right];

        Assert.That(arrows.Distinct().Count(), Is.EqualTo(4));
    }

    [Test]
    public void FunctionKeys_AreSequential()
    {
        int[] fKeys =
        [
            Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
            Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12
        ];

        for (int i = 1; i < fKeys.Length; i++)
            Assert.That(fKeys[i], Is.EqualTo(fKeys[i - 1] + 1),
                $"F{i + 1} should be F{i} + 1");
    }

    [Test]
    public void FunctionKeys_AreDistinct()
    {
        int[] fKeys =
        [
            Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
            Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12
        ];

        Assert.That(fKeys.Distinct().Count(), Is.EqualTo(12));
    }

    [Test]
    public void NavigationKeys_AreDistinct()
    {
        int[] navKeys =
        [
            Key.Home, Key.End, Key.PageUp, Key.PageDown,
            Key.Insert, Key.Delete
        ];

        Assert.That(navKeys.Distinct().Count(), Is.EqualTo(6));
    }

    [Test]
    public void AllKeys_ArePositive_ExceptErrKey()
    {
        int[] allKeys =
        [
            Key.Down, Key.Up, Key.Left, Key.Right,
            Key.Home, Key.End, Key.Backspace, Key.Enter,
            Key.PageUp, Key.PageDown, Key.Insert, Key.Delete,
            Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
            Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
            Key.Resize
        ];

        foreach (var key in allKeys)
            Assert.That(key, Is.GreaterThan(0), $"Key code 0x{key:X} should be positive");
    }

    [Test]
    public void AllKeys_AreDistinct()
    {
        int[] allKeys =
        [
            Key.ErrKey,
            Key.Down, Key.Up, Key.Left, Key.Right,
            Key.Home, Key.End, Key.Backspace, Key.Enter,
            Key.PageUp, Key.PageDown, Key.Insert, Key.Delete,
            Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
            Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
            Key.Resize
        ];

        Assert.That(allKeys.Distinct().Count(), Is.EqualTo(allKeys.Length));
    }

    [Test]
    public void SpecialKeys_HaveExpectedValues()
    {
        Assert.That(Key.Resize, Is.EqualTo(0x19A));
        Assert.That(Key.Enter, Is.EqualTo(0x157));
        Assert.That(Key.Backspace, Is.EqualTo(0x107));
    }
}
