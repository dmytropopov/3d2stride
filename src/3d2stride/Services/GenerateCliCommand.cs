/*
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace StrideGenerator.Services;

[Command(Name = "Generate", Description = "Generate output stride/indices files.")]
public sealed class GenerateCliCommand
{
    [FileExists]
    [Argument(0, Description = "Output file name. {0} for object name. {1} for zero-based index.", Name = "out-file")]
    public string OutputFileName { get; } = null!;

    [Option("-i|--input", CommandOptionType.MultipleValue, Description = "Input file name(s). Multiple options are supported.")]
    [Required]
    public string[] InputFileNames { get; } = default!;

    [Option("-s|--stride", CommandOptionType.SingleValue, Description = "Output stride format.")]
    public string OutputStrideFormat { get; } = "Vt0:F,UV0:F";

    [Option("-m|--merge", Description = "Merge all objects in input to one output.")]
    public bool MergeObjects { get; set; } = false;

    [Option("-a|--align", CommandOptionType.SingleValue, Description = "Output stride alignment. 0 for no alignment.")]
    public int OutputStrideAlignment { get; } = 4;

    [Option("-v|--verbosity", CommandOptionType.SingleValue, Description = "Verbosity.")]
    public Verbosity Verbosity { get; } = Verbosity.Normal;

    private readonly GlobalOptions _globalOptions;
    private readonly IGenerator _generator;
    private readonly ILogger<GenerateCliCommand> _logger;
    private readonly IConsole _console;

    public GenerateCliCommand(IGenerator generator, ILogger<GenerateCliCommand> logger, IConsole console, GlobalOptions globalOptions)
    {
        _globalOptions = globalOptions;
        _generator = generator;
        _logger = logger;
        _console = console;
    }

    public async Task OnExecuteAsync()
    {
        SetGlobalOptions();

        var inputSettings = InputFileNames.Select(s => new InputSettings
        {
            FileName = s,
            FileFormat = Constants.FileFormats.Obj
        });

        var outputSettings = new OutputSettings()
        {
            FileName = OutputFileName,
            OutputAttributes = OutputAttributes.Parse(OutputStrideFormat),
            MergeObjects = MergeObjects,
            Alignment = OutputStrideAlignment
        };
        _console.WriteLine("Output stride format: " + OutputStrideFormat);
        int strideSize = outputSettings.OutputAttributes.GetStrideSize();
        //if (OutputStrideAlignment != 0 && OutputStrideAlignment < strideSize)
        //{
        //    throw new Exception($"Alignment can't be less than stride data size ({strideSize}).");
        //}

        _console.WriteLine("Stride data size: " + strideSize);
        //_console.WriteLine("Aligned stride size: " + OutputStrideAlignment);

        if (string.IsNullOrEmpty(outputSettings.FileName))
        {
            outputSettings.FileName = Path.ChangeExtension(inputSettings.First().FileName, "").TrimEnd('.');
        }

        await _generator.Generate(inputSettings, outputSettings);
    }

    private void SetGlobalOptions()
    {
        _globalOptions.Verbosity = Verbosity;
    }
}
*/