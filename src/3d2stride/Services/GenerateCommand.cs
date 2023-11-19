using System.CommandLine;

namespace StrideGenerator.Services;

public class GenerateCommand : RootCommand
{
    private readonly GlobalOptions _globalOptions;
    private readonly IGenerator _generator;
    private readonly IConsole _console;

    public GenerateCommand(IGenerator generator, IConsole console, GlobalOptions globalOptions)
        : base("Generate output stride/indices files")
    {
        _globalOptions = globalOptions;
        _generator = generator;
        _console = console;

        var inputOption = new Option<string[]>("--input")
        {
            Description = "Input file name(s). Multiple options are supported",
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true,
        };
        inputOption.AddAlias("-i");

        var outputOption = new Option<string>("--out-file")
        {
            Description = "Output file name. {0} for object name. {1} for zero-based index",
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true,
        };
        outputOption.AddAlias("-o");

        var strideOption = new Option<string>("--stride")
        {
            Description = "Output stride format.",
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true,
        };
        strideOption.SetDefaultValue("Vt0:F,UV0:F");
        strideOption.AddAlias("-s");

        var alignOption = new Option<int>("--align")
        {
            Description = "Output stride alignment. 0 for no alignment",
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true,
        };
        alignOption.SetDefaultValue(4);
        alignOption.AddAlias("-a");

        var mergeOption = new Option<bool>("--merge")
        {
            Description = "Merge all objects in input to one output",
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true,
        };
        mergeOption.SetDefaultValue(false);
        mergeOption.AddAlias("-m");

        var verbosityOption = new Option<Verbosity>("--verbosity")
        {
            Description = "Verbosity",
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true,
        };
        verbosityOption.SetDefaultValue(Verbosity.Normal);
        verbosityOption.AddAlias("-v");

        AddOption(inputOption);
        AddOption(outputOption);
        AddOption(strideOption);
        AddOption(alignOption);
        AddOption(mergeOption);
        AddOption(verbosityOption);

        this.SetHandler(async (context) =>
        {
            var outputOptionValue = context.ParseResult.GetValueForOption(outputOption);
            var inputOptionValue = context.ParseResult.GetValueForOption(inputOption);
            var strideOptionValue = context.ParseResult.GetValueForOption(strideOption);
            var mergeOptionValue = context.ParseResult.GetValueForOption(mergeOption);
            var alignOptionValue = context.ParseResult.GetValueForOption(alignOption);
            var verbosityOptionValue = context.ParseResult.GetValueForOption(verbosityOption);

            _globalOptions.Verbosity = verbosityOptionValue;

            var inputSettings = inputOptionValue.Select(s => new InputSettings
            {
                FileName = s,
                FileFormat = Constants.FileFormats.Obj
            });

            var outputSettings = new OutputSettings()
            {
                FileName = outputOptionValue,
                OutputAttributes = OutputAttributes.Parse(strideOptionValue),
                MergeObjects = mergeOptionValue,
                Alignment = alignOptionValue
            };
            _console.WriteLine("Output stride format: " + strideOptionValue);
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
        });
    }
}