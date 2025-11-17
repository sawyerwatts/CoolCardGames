using System.ComponentModel.DataAnnotations;

namespace CoolCardGames.Cli;

public class CliPlayerUserSettings
{
    [Range(0, 1000 * 5)] public int MillisecondDelayBetweenWritingMessagesToConsole { get; set; } = 300;
}