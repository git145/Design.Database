using Microsoft.Extensions.Configuration;
using roundhouse;
using roundhouse.infrastructure.logging.custom;
using System.Reflection;

const string keyConnectionString =
    "ConnectionString:Design";

const string keyScriptRoot =
    "ScriptRoot";

const string keyEnvironment =
    "Environment";

const string keyTimeoutSecond =
    "TimeoutSecond";

const string keyDropBeforeRun =
    "DropBeforeRun";

ConfigurationBuilder configurationBuilder =
    new();

configurationBuilder.AddJsonFile("appsettings.json",
    optional: false);

IConfiguration configuration = 
    configurationBuilder.Build();

string? connectionString = ReadSetting(configuration,
    keyConnectionString);

if (string.IsNullOrWhiteSpace(connectionString))
{
    LogConfigurationMissing(keyConnectionString);

    return;
}

string? scriptRoot = ReadSetting(configuration,
    keyScriptRoot);

if (string.IsNullOrWhiteSpace(scriptRoot))
{
    LogConfigurationMissing(keyScriptRoot);

    return;
}

string? environment = ReadSetting(configuration,
    keyEnvironment);

if (string.IsNullOrWhiteSpace(environment))
{
    LogConfigurationMissing(keyEnvironment);

    return;
}

string? timeoutSecondString = ReadSetting(configuration,
    keyTimeoutSecond);

if (string.IsNullOrWhiteSpace(timeoutSecondString))
{
    LogConfigurationMissing(keyTimeoutSecond);

    return;
}

string? dropBeforeRunString = ReadSetting(configuration,
    keyDropBeforeRun);

if (string.IsNullOrWhiteSpace(dropBeforeRunString))
{
    LogConfigurationMissing(keyDropBeforeRun);

    return;
}

bool isTimeoutSecondValid = int.TryParse(timeoutSecondString, 
    out int timeoutSecond);

if (!isTimeoutSecondValid)
{
    LogConfigurationInvalid(keyTimeoutSecond);

    return;
}

bool dropBeforeRun = 
    Convert.ToBoolean(dropBeforeRunString);

MigrationHelper(connectionString,
    scriptRoot,
    environment,
    dropBeforeRun,
    timeoutSecond);

static string? ReadSetting(IConfiguration configuration, 
    string key)
{
    string? result =
        string.Empty;

    try
    {
        return configuration
            .GetSection(key)
            .Value;
    }
    catch (Exception exception)
    {
        Console.WriteLine("Error reading configuration: {exception}",
            exception);

        return null;
    }
}

static void LogConfigurationMissing(string key)
{
    Console.WriteLine("The configuration value {key} is missing",
        key);
}

static void LogConfigurationInvalid(string key)
{
    Console.WriteLine("The configuration value {key} is invalid",
        key);
}

static void MigrationHelper(string connectionString,
    string scriptLocation,
    string environment,
    bool dropBeforeRun,
    int timeoutSecond)
{
    Migrate migration =
        new();

    migration.Set(x =>
    {
        x.ConnectionString =
            connectionString;

        Version? version = Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version;

        if (version != null)
        {
            x.Version =
                version.ToString();
        }

        x.RepositoryPath =
            string.Empty;

        x.SqlFilesDirectory = 
            scriptLocation;

        x.Silent =
            true;

        x.Logger =
            new ConsoleLogger();

        x.CommandTimeout =
            timeoutSecond;

        x.EnvironmentNames
            .Add(environment);

        x.WarnOnOneTimeScriptChanges = 
            true;
    });

    if (dropBeforeRun)
    {
        migration.RunDropCreate();

        return;
    }

    migration.Run();
}
