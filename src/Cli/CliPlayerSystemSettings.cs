using System.ComponentModel.DataAnnotations;

namespace CoolCardGames.Cli;

public class CliPlayerSystemSettings
{
    [Range(0, 1000)] public int MillisecondDelayBetweenCheckingIfCliIsUpToDateOnEvents { get; set; } = 100;
}