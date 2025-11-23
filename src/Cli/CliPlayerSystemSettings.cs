using System.ComponentModel.DataAnnotations;

namespace CoolCardGames.Cli;

public class CliPlayerSystemSettings
{
    [Range(0, 1000)] public int NumTriesToConnectToGame { get; set; } = 10;
    [Range(0, 1000)] public int MillisecondDelayBetweenCheckingIfCliIsUpToDateOnEvents { get; set; } = 100;
}