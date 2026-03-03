namespace BCyT.NetCurses.Tests;

public class AttrsTests
{
    [Test]
    public void Normal_IsZero()
    {
        Assert.That(Attrs.Normal, Is.EqualTo(0u));
    }

    [Test]
    public void AttributeConstants_AreDistinctBitFlags()
    {
        uint[] flags =
        [
            Attrs.Standout, Attrs.Underline, Attrs.Reverse, Attrs.Blink,
            Attrs.Dim, Attrs.Bold, Attrs.Invisible, Attrs.Italic
        ];

        // Each flag should be a single bit (power of two)
        foreach (var flag in flags)
            Assert.That(flag & (flag - 1), Is.EqualTo(0u), $"Flag 0x{flag:X8} is not a power of two");

        // All flags should be distinct
        Assert.That(flags.Distinct().Count(), Is.EqualTo(flags.Length));
    }

    [Test]
    public void AttributeConstants_AreInBits16AndAbove()
    {
        uint[] flags =
        [
            Attrs.Standout, Attrs.Underline, Attrs.Reverse, Attrs.Blink,
            Attrs.Dim, Attrs.Bold, Attrs.Invisible, Attrs.Italic
        ];

        foreach (var flag in flags)
            Assert.That(flag, Is.GreaterThanOrEqualTo(0x00010000u),
                $"Flag 0x{flag:X8} should be in bits 16+");
    }

    [Test]
    public void AttributeFlags_CanBeCombined()
    {
        uint combined = Attrs.Bold | Attrs.Underline;

        Assert.That(combined & Attrs.Bold, Is.Not.EqualTo(0u));
        Assert.That(combined & Attrs.Underline, Is.Not.EqualTo(0u));
        Assert.That(combined & Attrs.Italic, Is.EqualTo(0u));
    }

    [Test]
    public void ColorPair_EncodesInBits8To15()
    {
        uint result = Attrs.ColorPair(1);

        Assert.That(result, Is.EqualTo(0x00000100u));
    }

    [Test]
    public void ColorPair_Zero_ReturnsZero()
    {
        Assert.That(Attrs.ColorPair(0), Is.EqualTo(0u));
    }

    [Test]
    public void ColorPair_MaxValue_Encodes255()
    {
        uint result = Attrs.ColorPair(255);

        Assert.That(result, Is.EqualTo(0x0000FF00u));
    }

    [Test]
    public void ColorPair_MasksToLowByte()
    {
        // Values above 255 should be masked to low byte
        uint result = Attrs.ColorPair(256);

        Assert.That(result, Is.EqualTo(0u)); // 256 & 0xFF == 0
    }

    [Test]
    public void PairNumber_ExtractsFromBits8To15()
    {
        int pair = Attrs.PairNumber(0x00000300u); // pair 3

        Assert.That(pair, Is.EqualTo(3));
    }

    [Test]
    public void PairNumber_IgnoresOtherBits()
    {
        // Bold | color pair 5
        uint attr = Attrs.Bold | Attrs.ColorPair(5);
        int pair = Attrs.PairNumber(attr);

        Assert.That(pair, Is.EqualTo(5));
    }

    [Test]
    public void PairNumber_ZeroAttributes_ReturnsZero()
    {
        Assert.That(Attrs.PairNumber(0), Is.EqualTo(0));
    }

    [Test]
    public void ColorPair_PairNumber_Roundtrip()
    {
        for (int i = 0; i < 256; i++)
        {
            uint encoded = Attrs.ColorPair(i);
            int decoded = Attrs.PairNumber(encoded);
            Assert.That(decoded, Is.EqualTo(i), $"Roundtrip failed for pair {i}");
        }
    }

    [Test]
    public void ColorPair_DoesNotOverlapWithAttributeFlags()
    {
        uint colorBits = Attrs.ColorPair(255); // All color bits set
        uint[] flags =
        [
            Attrs.Standout, Attrs.Underline, Attrs.Reverse, Attrs.Blink,
            Attrs.Dim, Attrs.Bold, Attrs.Invisible, Attrs.Italic
        ];

        foreach (var flag in flags)
            Assert.That(colorBits & flag, Is.EqualTo(0u),
                $"Color pair bits overlap with flag 0x{flag:X8}");
    }
}
