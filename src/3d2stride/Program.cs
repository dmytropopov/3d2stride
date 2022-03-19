using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StrideGenerator.Services;

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
            .AddSingleton<IConsole>(PhysicalConsole.Singleton);
    })
    .RunCommandLineApplicationAsync<GenerateCliCommand>(args);
