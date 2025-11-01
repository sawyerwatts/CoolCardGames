using System.Threading.Channels;

using CoolCardGames.Library.Core.MiscUtils;

using Microsoft.Extensions.Logging.Abstractions;

namespace CoolCardGames.XUnitTests.Library.Core.MiscUtils;

public class ChannelFanOutHandlerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Test(int numReaders)
    {
        if (numReaders < 0)
            throw new ArgumentException($"{nameof(numReaders)} cannot be negative");

        var source = Channel.CreateUnbounded<string>(new UnboundedChannelOptions()
        {
            SingleWriter = true,
            SingleReader = true,
        });
        var sut = new ChannelFanOutHandler<string>(source.Reader, NullLogger<ChannelFanOutHandler<string>>.Instance);

        var readers = new List<ChannelReader<string>>(capacity: numReaders);
        for (int i = 0; i < numReaders; i++)
        {
            var reader = sut.CreateReader($"reader {i}");
            readers.Add(reader);
        }

        var msg0 = "foo";
        var msg1 = "bar";
        var msg2 = "baz";
        await source.Writer.WriteAsync(msg0);
        await source.Writer.WriteAsync(msg1);
        await source.Writer.WriteAsync(msg2);
        source.Writer.Complete();

        await sut.HandleFanout(CancellationToken.None);

        foreach (var reader in readers)
        {
            Assert.True(reader.TryRead(out string? actualMsg0));
            Assert.Equal(msg0, actualMsg0);

            Assert.True(reader.TryRead(out string? actualMsg1));
            Assert.Equal(msg1, actualMsg1);

            Assert.True(reader.TryRead(out string? actualMsg2));
            Assert.Equal(msg2, actualMsg2);

            Assert.False(reader.TryRead(out string? _));
        }
    }
}