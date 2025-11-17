using System.ComponentModel.DataAnnotations;

namespace CoolCardGames.Cli;

public class CliPlayerSettings
{
    [Range(0, 1000_5)] public int MillisecondDelayBetweenWritingMessagesToConsole { get; set; } = 300;
}