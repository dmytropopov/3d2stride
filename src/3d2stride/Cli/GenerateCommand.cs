using System.CommandLine;
using StrideGenerator.Data;
using StrideGenerator.Services;

namespace StrideGenerator.Cli;

public class GenerateCommand : RootCommand
{
    private readonly GlobalOptions _globalOptions;
    private readonly IGenerator _generator;
    private readonly Services.IConsole _console;

    public GenerateCommand(IGenerator generator, Services.IConsole console, GlobalOptions globalOptions)
        : base("Generate output stride/indices files")
    {
        var supportedAttributesHelpText = GenerateSupportedEnumsHelpText(CliConstants.AttributeInfos, x => x.HelpText);
        var supportedFormatsHelpText = GenerateSupportedEnumsHelpText(CliConstants.FormatInfos, x => $"{x.HelpText}, {Constants.FormatSizes[x.AttributeFormat]} {(Constants.FormatSizes[x.AttributeFormat] == 1 ? "byte" : "bytes")}.");
        var supportedProcessingsHelpText = GenerateSupportedEnumsHelpText(CliConstants.ProcessingInfos, x => x.HelpText);
        var supportedIndexFormatsHelpText = GenerateSupportedEnumsHelpText(CliConstants.IndexFormatInfos, x => x.HelpText);

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
  <attribute><input-index>:<format>
Bit-packed formats can have multiple attributes inside, separated by '+' sign:
  <attribute><input-index>[+<attribute><input-index>...+<attribute><input-index>]:<format>
Example: E0+VTX0+VTY0+VTZ0:SI2101010,UV0:F
Supported attributes:
{supportedAttributesHelpText}
<input-index> is zero-based.
Supported output formats:
{supportedFormatsHelpText}
",
            IsRequired = true
        };
        strideOption.AddAlias("-s");

        var processOption = new Option<string>("--process")
        {
            Description = @$"Atttibute processing:
  <attribute><input-index>:<processing>
Example: N0:NORMV3,V0:ONEMIN
Processing types:
{supportedProcessingsHelpText}
<input-index> is zero-based.
",
            IsRequired = false
        };
        processOption.AddAlias("-p");

        var indexFormatOption = new Option<string>("--index-format")
        {
            Description = @$"Output index format:
{supportedIndexFormatsHelpText}
",
            IsRequired = false
        };
        indexFormatOption.AddAlias("-if");
        indexFormatOption.SetDefaultValue("S");

        var alignOption = new Option<int>("--align")
        {
            Description = "Output stride alignment. 0 for no alignment",
            IsRequired = false
        };
        alignOption.SetDefaultValue(4);
        alignOption.AddAlias("-a");

        var mergeOption = new Option<bool>("--merge")
        {
            Description = "Merge all objects in input to one output",
            IsRequired = false
        };
        mergeOption.SetDefaultValue(false);
        mergeOption.AddAlias("-m");

        var boundingBoxOutputOption = new Option<BoundingBoxOutputType>("--bounding-box-output")
        {
            Description = "Bounding box output",
            IsRequired = false
        };
        boundingBoxOutputOption.AddAlias("-bbo");

        var verbosityOption = new Option<Verbosity>("--verbosity")
        {
            Description = "Verbosity",
            IsRequired = false
        };
        verbosityOption.SetDefaultValue(Verbosity.Normal);
        verbosityOption.AddAlias("-v");

        AddOption(inputOption);
        AddOption(outputOption);
        AddOption(strideOption);
        AddOption(processOption);
        AddOption(indexFormatOption);
        AddOption(alignOption);
        AddOption(mergeOption);
        AddOption(boundingBoxOutputOption);
        AddOption(verbosityOption);

        this.SetHandler(async (context) =>
        {
            var outputOptionValue = context.ParseResult.GetValueForOption(outputOption);
            var inputOptionValue = context.ParseResult.GetValueForOption(inputOption);
            var strideOptionValue = context.ParseResult.GetValueForOption(strideOption);
            var processOptionValue = context.ParseResult.GetValueForOption(processOption);
            var indexFormatOptionValue = context.ParseResult.GetValueForOption(indexFormatOption);
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
                StrideMap = StrideParameterParser.Parse(strideOptionValue!),
                ProcessingMap = ProcessingParameterParser.Parse(processOptionValue!),
                MergeObjects = mergeOptionValue,
                Alignment = alignOptionValue
            };
            int strideSize = outputSettings.GetStrideSize();

            var strideFormat = string.Join(',', outputSettings.StrideMap.Select(StridePieceToString));
            _console.WriteLine($"Stride: {strideSize} byte(s) {strideFormat}");

            if (string.IsNullOrEmpty(outputSettings.FileName))
            {
                outputSettings.FileName = Path.ChangeExtension(inputSettings.First().FileName, "").TrimEnd('.');
            }

            await _generator.Generate(inputSettings, outputSettings);
        });
    }

    private static string GenerateSupportedEnumsHelpText<T>(Dictionary<string, T> dictionary, Func<T, string> helpTextFunc)
    {
        return string.Join(Environment.NewLine, dictionary.Select(s => $"  {s.Key}: {helpTextFunc(s.Value)}"));
    }

    private static string StridePieceToString(StridePiece stridePiece) => $"{AttributeTypesToString(stridePiece.AttributeTypesPerInput)}:{CliConstants.FormatInfos.Single(af => af.Value.AttributeFormat == stridePiece.Format).Key}({stridePiece.Offset})";

    private static string AttributeTypesToString(List<FormatComponent> attributeTypesPerInput) => string.Join("+", attributeTypesPerInput.Select(x => $"{CliConstants.AttributeInfos.Single(ai => ai.Value.AttributeComponentType == x.AttributeComponentType).Key}{x.InputIndex}"));
}