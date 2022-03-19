using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace StrideGenerator.Services;

[Command(Name = "Generate", Description = "Generate output stride/indices files.")]
public class GenerateCliCommand
{
    [Argument(0, Description = "Output file name", Name = "out-file")]
    public string OutputFileName { get; }

    [Option("-i|--input", CommandOptionType.MultipleValue, Description = "Input file name(s). Multiple options are supported.")]
    [Required]
    public string[] InputFileNames { get; } = default!;

    private readonly IGenerator _generator;
    private readonly ILogger<GenerateCliCommand> _logger;

    public GenerateCliCommand(IGenerator generator, ILogger<GenerateCliCommand> logger)
    {
        _generator = generator;
        _logger = logger;
    }

    public async Task OnExecuteAsync()
    {
        var inputSettings = InputFileNames.Select(s => new InputSettings
        {
            FileName = s,
            FileFormat = Constants.FileFormats.Obj
        });
        //foreach (var inputFileName in InputFileNames)
        //{
        //    _logger.LogInformation($"Input file: {inputFileName}");
        //}

        await _generator.Generate(inputSettings, new OutputSettings { FileName = OutputFileName });
    }
}