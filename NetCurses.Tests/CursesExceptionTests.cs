namespace BCyT.NetCurses.Tests;

public class CursesExceptionTests
{
    [Test]
    public void DefaultConstructor_CreatesException()
    {
        var ex = new CursesException();

        Assert.That(ex, Is.InstanceOf<Exception>());
    }

    [Test]
    public void MessageConstructor_SetsMessage()
    {
        var ex = new CursesException("test error");

        Assert.That(ex.Message, Is.EqualTo("test error"));
    }

    [Test]
    public void InnerExceptionConstructor_SetsBoth()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new CursesException("outer", inner);

        Assert.That(ex.Message, Is.EqualTo("outer"));
        Assert.That(ex.InnerException, Is.SameAs(inner));
    }

    [Test]
    public void CanBeCaughtAsException()
    {
        Assert.Throws<CursesException>(() => throw new CursesException("fail"));
    }

    [Test]
    public void CanBeCaughtAsBaseException()
    {
        Assert.That(() => throw new CursesException("fail"),
            Throws.InstanceOf<Exception>());
    }
}
