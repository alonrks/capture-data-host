using System.CommandLine;
using CaptureDataHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using SqlKata.Compilers;
using SqlKata.Execution;


// Set up configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("./Config/sqlite.json")
    .Build();

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole( options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = false;
        options.TimestampFormat = "HH:mm:ss ";
    });
});
// Set up dependency injection
var serviceProvider = new ServiceCollection()
    .AddTransient<QueryFactory>(sp =>
    {
        var connection = new SQLiteConnection(
            configuration.GetConnectionString("DefaultConnection")
        );

        var compiler = new SqliteCompiler();

        return new QueryFactory(connection, compiler);

    })
    .AddTransient<ISqliteRepository, SqliteRepository>()
    .AddScoped<ILogger<HostConsumer>>( sp => loggerFactory.CreateLogger<HostConsumer>())
    .AddScoped<ILogger<HostProducer>>( sp => loggerFactory.CreateLogger<HostProducer>())
    .AddScoped<IHostConsumer, HostConsumer>()
    .AddScoped<IHostProducer, HostProducer>()
    .BuildServiceProvider();

var hostOption = new Option<IList<IHostWorker>>(
    name: "--host",
    description: "Types of the hosts to be executed.",
    parseArgument: result =>
    {
        var hosts = new List<IHostWorker>();
        if (result.Tokens.Count == 0)
        {
            hosts.Add(serviceProvider.GetService<IHostConsumer>() ?? throw new InvalidOperationException("Cannot get service."));
            hosts.Add(serviceProvider.GetService<IHostProducer>() ?? throw new InvalidOperationException("Cannot get service."));
        }
        else
        {
            var token = result.Tokens[0];
            if (Enum.TryParse<HostMode>(token.Value, true, out var mode))
            {
                switch (mode)
                {
                    case HostMode.Consumer:
                        hosts.Add(serviceProvider.GetService<IHostConsumer>() ??
                                  throw new InvalidOperationException("Cannot get service."));
                        break;
                    case HostMode.Producer:
                        hosts.Add(serviceProvider.GetService<IHostProducer>() ??
                                  throw new InvalidOperationException("Cannot get service."));
                        break;
                    case HostMode.Both:
                    default:
                        hosts.Add(serviceProvider.GetService<IHostConsumer>() ??
                                  throw new InvalidOperationException("Cannot get service."));
                        hosts.Add(serviceProvider.GetService<IHostProducer>() ??
                                  throw new InvalidOperationException("Cannot get service."));
                        break;
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot parse host mode.");
            }
        }

        return hosts;
    });
var workloadOption = new Option<int>(
    name: "--workload",
    description: "Defines the workload.",
    getDefaultValue: () => 10);

var rootCommand = new RootCommand("The console application for the Host.");
var runCommand = new Command("run", "Run host.")
{
    hostOption,
    workloadOption
}; 
rootCommand.AddCommand(runCommand);


runCommand.SetHandler((hosts, workload) =>
    {
        try
        {
            var tasks = hosts.Select(host => Task.Run(() => host.RunAsync(workload))).ToList();
            return Task.WhenAll(tasks.ToArray());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return Task.CompletedTask;
    }, hostOption, workloadOption);

await rootCommand.InvokeAsync(args);
Console.WriteLine("done");