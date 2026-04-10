using System.Diagnostics;

namespace CoolCardGames.WebApi;

public class AppLevelCancellationTokenHostedService : IHostedService
{
    public CancellationToken Token => _token ?? throw new UnreachableException("Not sure how this happened");

    private CancellationToken? _token;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _token = cancellationToken;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}