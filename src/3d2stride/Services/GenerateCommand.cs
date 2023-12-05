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
        var supportedAttributesHelpText = String.Join(Environment.NewLine, OutputAttributes.AttributesInfos.Select(s => $"  - {s.Key}: {s.Value.HelpText}"));
        var supportedFormatsHelpText = String.Join(Environment.NewLine, OutputAttributes.AttributeFormats.Select(s => $"  - {s.Key}: {s.Value.HelpText}, {OutputAttributes.FormatSizes[s.Value.AttributeFormat]} byte(s)."));

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
            Description = @$"Output stride format, separated by comma:
  <attribute><input-index>:<format>.
Bit-packed formats can have multiple attributes inside, separated by '+' sign:
  <attribute><input-index>[+<attribute><input-index>...+<attribute><input-index>]:<format>.
Example: E0+VTX0+VTY0+VTZ0:SI2101010,UV0:F
Supported attributes:
{supportedAttributesHelpText}
<input-index> is zero-based.
Supported formats:
{supportedFormatsHelpText}
",
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true,
        };
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

            var inputSettings = inputOptionValue!.Select(s => new InputSettings
            {
                FileName = s,
                FileFormat = Constants.FileFormats.Obj
            });

            var outputSettings = new OutputSettings()
            {
                FileName = outputOptionValue!,
                OutputAttributes = OutputAttributes.Parse(strideOptionValue!),
                MergeObjects = mergeOptionValue,
                Alignment = alignOptionValue
            };
            _console.WriteLine("Output stride format: " + strideOptionValue);
            int strideSize = outputSettings.OutputAttributes.GetStrideSize();
            //if (OutputStrideAlignment != 0 && OutputStrideAlignment < strideSize)
            //{
            //    throw new Exception($"Alignment can't be less than stride data size ({strideSize}).");
            //}

            var strideFormat = string.Join(',', outputSettings.OutputAttributes.StrideMap.Select(sp => $"{string.Join('+', sp.AttributeTypes.Select(at => OutputAttributes.AttributesInfos.Single(ai => ai.Value.AttributeType == at).Key))}{sp.InputIndex}:{OutputAttributes.AttributeFormats.Single(af => af.Value.AttributeFormat == sp.Format).Key}"));
            _console.WriteLine($"Stride: {strideSize} byte(s) {strideFormat}");

            if (string.IsNullOrEmpty(outputSettings.FileName))
            {
                outputSettings.FileName = Path.ChangeExtension(inputSettings.First().FileName, "").TrimEnd('.');
            }

            await _generator.Generate(inputSettings, outputSettings);
        });
    }
}