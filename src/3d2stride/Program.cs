using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StrideGenerator.Services;
using StrideGenerator.Services.Obj;
using Scrutor;

return await new HostBuilder()
    .ConfigureLogging((context, builder) =>
    {
        builder.AddConsole();
    })
    .ConfigureHostConfiguration(configure =>
    {
        //configure.
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IGenerator, Generator>()
            .AddSingleton<InputReaderFactory>()
            .AddSingleton<IGenerator, Generator>()
            .AddSingleton<IInputReader, ObjReader>()
            .AddSingleton<IOutputWriter, OutputWriter>()
            .AddSingleton<MeshOptimizer>()
            .AddSingleton(PhysicalConsole.Singleton)
            .AddSingleton<GlobalOptions>()
            .Decorate<IConsole, VerbosityConsoleDecorator>();
    })
    .RunCommandLineApplicationAsync<GenerateCliCommand>(args);
