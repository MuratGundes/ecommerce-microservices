using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Test.Helpers;

public class OptionsHelper
{
    public static TSettings GetOptions<TSettings>(string section, string settingsFileName = null)
        where TSettings : class, new()
    {
        settingsFileName ??= "appsettings.test.json";
        var configuration = new TSettings();

        GetConfigurationRoot(settingsFileName)
            .GetSection(section)
            .Bind(configuration);

        return configuration;
    }

    public static string GetConnectionString(string connectionName, string settingsFileName = null)
    {
        settingsFileName ??= "appsettings.test.json";

        var config = GetConfigurationRoot(settingsFileName);
        var value = config.GetSection("ConnectionStrings")[connectionName];

        return value;
    }

    private static IConfigurationRoot GetConfigurationRoot(string settingsFileName)
    {
        return new ConfigurationBuilder()
            .AddJsonFile(settingsFileName, true)
            .AddEnvironmentVariables()
            .Build();
    }
}
