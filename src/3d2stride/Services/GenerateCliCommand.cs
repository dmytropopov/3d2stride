﻿using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace StrideGenerator.Services;

[Command(Name = "Generate", Description = "Generate output stride/indices files.")]
public sealed class GenerateCliCommand
{
    [Argument(0, Description = "Output file name. {0} for object name. {1} for zero-based index.", Name = "out-file")]
    public string OutputFileName { get; }

    [Option("-i|--input", CommandOptionType.MultipleValue, Description = "Input file name(s). Multiple options are supported.")]
    [Required]
    public string[] InputFileNames { get; } = default!;

    [Option("-s|--stride", CommandOptionType.SingleValue, Description = "Output stride format.")]
    public string OutputStrideFormat { get; } = "Vt0:F,UV0:F";

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

        var outputSettings = new OutputSettings()
        {
            FileName = OutputFileName,
            OutputAttributes = OutputAttributes.Parse(OutputStrideFormat)
        };
        Console.WriteLine("Output stride format: " + OutputStrideFormat);
        Console.WriteLine("Output stride size: " + outputSettings.OutputAttributes.GetStrideSize());

        if (string.IsNullOrEmpty(outputSettings.FileName))
        {
            outputSettings.FileName = Path.ChangeExtension(inputSettings.First().FileName, "").TrimEnd('.');
        }

        await _generator.Generate(inputSettings, outputSettings);
    }
}