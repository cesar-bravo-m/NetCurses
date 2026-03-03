using System.Text;

namespace BCyT.NetCurses.Tests;

public class CharInfoTests
{
    [Test]
    public void Constructor_SetsFields()
    {
        var ci = new CharInfo(new Rune('A'), Attrs.Bold);

        Assert.That(ci.Character, Is.EqualTo(new Rune('A')));
        Assert.That(ci.Attributes, Is.EqualTo(Attrs.Bold));
    }

    [Test]
    public void Blank_IsSpaceWithNormalAttrs()
    {
        Assert.That(CharInfo.Blank.Character, Is.EqualTo(new Rune(' ')));
        Assert.That(CharInfo.Blank.Attributes, Is.EqualTo(Attrs.Normal));
    }

    [Test]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = new CharInfo(new Rune('X'), Attrs.Underline);
        var b = new CharInfo(new Rune('X'), Attrs.Underline);

        Assert.That(a.Equals(b), Is.True);
    }

    [Test]
    public void Equals_DifferentCharacter_ReturnsFalse()
    {
        var a = new CharInfo(new Rune('X'), Attrs.Normal);
        var b = new CharInfo(new Rune('Y'), Attrs.Normal);

        Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Equals_DifferentAttributes_ReturnsFalse()
    {
        var a = new CharInfo(new Rune('X'), Attrs.Normal);
        var b = new CharInfo(new Rune('X'), Attrs.Bold);

        Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Equals_Object_SameValues_ReturnsTrue()
    {
        var a = new CharInfo(new Rune('A'), Attrs.Bold);
        object b = new CharInfo(new Rune('A'), Attrs.Bold);

        Assert.That(a.Equals(b), Is.True);
    }

    [Test]
    public void Equals_Object_WrongType_ReturnsFalse()
    {
        var a = new CharInfo(new Rune('A'), Attrs.Bold);

        Assert.That(a.Equals("not a CharInfo"), Is.False);
    }

    [Test]
    public void Equals_Object_Null_ReturnsFalse()
    {
        var a = new CharInfo(new Rune('A'), Attrs.Bold);

        Assert.That(a.Equals(null), Is.False);
    }

    [Test]
    public void EqualityOperator_SameValues_ReturnsTrue()
    {
        var a = new CharInfo(new Rune('Z'), Attrs.Reverse);
        var b = new CharInfo(new Rune('Z'), Attrs.Reverse);

        Assert.That(a == b, Is.True);
    }

    [Test]
    public void InequalityOperator_DifferentValues_ReturnsTrue()
    {
        var a = new CharInfo(new Rune('Z'), Attrs.Reverse);
        var b = new CharInfo(new Rune('Z'), Attrs.Bold);

        Assert.That(a != b, Is.True);
    }

    [Test]
    public void GetHashCode_SameValues_SameHash()
    {
        var a = new CharInfo(new Rune('H'), Attrs.Italic);
        var b = new CharInfo(new Rune('H'), Attrs.Italic);

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void GetHashCode_DifferentValues_DifferentHash()
    {
        var a = new CharInfo(new Rune('H'), Attrs.Italic);
        var b = new CharInfo(new Rune('I'), Attrs.Italic);

        // Not strictly guaranteed but extremely likely for different inputs
        Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void DefaultStruct_HasZeroValues()
    {
        var ci = new CharInfo();

        Assert.That(ci.Character, Is.EqualTo(new Rune(0)));
        Assert.That(ci.Attributes, Is.EqualTo(0u));
    }

    [Test]
    public void UnicodeCharacter_PreservedCorrectly()
    {
        var rune = new Rune('★');
        var ci = new CharInfo(rune, Attrs.Normal);

        Assert.That(ci.Character, Is.EqualTo(rune));
    }
}
