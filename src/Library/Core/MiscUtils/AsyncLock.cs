using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.MiscUtils;

/// <summary>
/// The `lock (_lock) {}` syntax doesn't work if the block is asyncronous, so this type exists to
/// make that easy.
/// </summary>
/// <param name="logger"></param>
public partial class AsyncLock(ILogger logger)
{
    private readonly Lock _lock = new();

    public async Task<T> LockThenExecute<T>(string callerMethodName, Func<Task<T>> f)
    {
        LogTryingToEnterLockForMethodName(callerMethodName);
        _lock.Enter();
        LogEnteredLockForMethodName(callerMethodName);
        try
        {
            return await f();
        }
        finally
        {
            LogExitingLockForMethodName(callerMethodName);
            _lock.Exit();
            LogExitedLockForMethodName(callerMethodName);
        }
    }

    [LoggerMessage(LogLevel.Information, "Trying to enter lock for {MethodName}")]
    partial void LogTryingToEnterLockForMethodName(string methodName);

    [LoggerMessage(LogLevel.Information, "Entered lock for {MethodName}")]
    partial void LogEnteredLockForMethodName(string methodName);

    [LoggerMessage(LogLevel.Information, "Exiting lock for {MethodName}")]
    partial void LogExitingLockForMethodName(string methodName);

    [LoggerMessage(LogLevel.Information, "Exited lock for {MethodName}")]
    partial void LogExitedLockForMethodName(string methodName);
}