using CoolCardGames.Library.Core.MiscUtils;

namespace CoolCardGames.XUnitTests.Library.Core.MiscUtils;

public class CircularCounterTests
{
    [Fact]
    public void TestCyclingUp()
    {
        CircularCounter sut = new(3);
        Assert.Equal(0, sut.N);

        Assert.Equal(1, sut.Tick());
        Assert.Equal(1, sut.N);

        Assert.Equal(2, sut.Tick());
        Assert.Equal(2, sut.N);

        Assert.Equal(0, sut.Tick());
        Assert.Equal(0, sut.N);
    }

    [Fact]
    public void TestCyclingDown()
    {
        CircularCounter sut = new(3, startAtEnd: true);
        Assert.Equal(2, sut.N);

        Assert.Equal(1, sut.Tick(-1));
        Assert.Equal(1, sut.N);

        Assert.Equal(0, sut.Tick(-1));
        Assert.Equal(0, sut.N);

        Assert.Equal(2, sut.Tick(-1));
        Assert.Equal(2, sut.N);
    }

    [Fact]
    public void TestTickDontUpdateCounter()
    {
        CircularCounter sut = new(3);
        Assert.Equal(0, sut.N);

        Assert.Equal(1, sut.Tick(updateInstance: false));
        Assert.Equal(0, sut.N);
    }

    [Fact]
    public void TestMultiTickThatDontRollOver()
    {
        CircularCounter sut = new(3);
        Assert.Equal(0, sut.N);

        Assert.Equal(2, sut.Tick(2));
        Assert.Equal(2, sut.N);

        Assert.Equal(0, sut.Tick());
        Assert.Equal(0, sut.N);
    }

    [Fact]
    public void TestMultiTickThatRollOver()
    {
        CircularCounter sut = new(3);
        Assert.Equal(0, sut.N);

        Assert.Equal(1, sut.Tick(4));
        Assert.Equal(1, sut.N);
    }
}