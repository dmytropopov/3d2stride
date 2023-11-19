using Microsoft.Extensions.DependencyInjection;
using StrideGenerator.Services;
using StrideGenerator.Services.Obj;
using System.CommandLine;
using Microsoft.Extensions.Configuration;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        var serviceProvider = BuildServiceProvider();
        var parser = BuildParser(serviceProvider);

        return await parser.InvokeAsync(args).ConfigureAwait(false);
    }

    private static Parser BuildParser(ServiceProvider serviceProvider)
    {
        var commandLineBuilder = new CommandLineBuilder(serviceProvider.GetService<GenerateCommand>());
        return commandLineBuilder.UseDefaults().Build();
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<GenerateCommand>();

        services.AddSingleton<IGenerator, Generator>()
            .AddSingleton<InputReaderFactory>()
            .AddSingleton<IGenerator, Generator>()
            .AddSingleton<IInputReader, ObjReader>()
            .AddSingleton<IOutputWriter, OutputWriter>()
            .AddSingleton<MeshOptimizer>()
            .AddSingleton<StrideGenerator.Services.IConsole, SystemConsole>()
            .AddSingleton<GlobalOptions>();
            //.Decorate<StrideGenerator.Services.IConsole, VerbosityConsoleDecorator>();

        return services.BuildServiceProvider();
    }
}