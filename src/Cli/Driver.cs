using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

namespace CoolCardGames.Cli;

public class Driver(
    IOptions<Driver.Settings> settings,
    ILogger<Driver> logger)
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        // TODO: start coding here
        logger.LogInformation("Start coding here");
        return Task.CompletedTask;
    }

    public static void RegisterTo(
        IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<Driver>();
        builder.Services.AddSingleton<IValidateOptions<Settings>, ValidateDriverSettings>();
        builder.Services.AddOptions<Settings>()
            .Bind(builder.Configuration.GetRequiredSection("Driver"))
            .ValidateOnStart();
    }

    public class Settings
    {
        [Required]
        public string Demo { get; set; } = "";
    }
}

[OptionsValidator]
public partial class ValidateDriverSettings : IValidateOptions<Driver.Settings>;
