using Microsoft.Extensions.Logging;

namespace StrideGenerator.Services;

public class Generator : IGenerator
{
    private readonly ILogger _logger;
    private readonly InputReaderFactory _inputReaderFactory;
    private readonly IOutputWriter _outputWriter;

    public Generator(ILogger<Generator> logger, InputReaderFactory inputReaderFactory, IOutputWriter outputWriter)
    {
        _logger = logger;
        _inputReaderFactory = inputReaderFactory;
        _outputWriter = outputWriter;
    }

    public async Task Generate(IEnumerable<InputSettings> inputs, OutputSettings output)
    {
        // TODO
        // When output is not specified and there is only 1 object in input, use input file name
        // If there are multiple objects in input file, write them all and use object names as output file names
        foreach (var inputSettings in inputs)
        {
            var reader = _inputReaderFactory.GetReader(inputSettings.FileFormat);
            var meshes = await reader.ReadInput(inputSettings);

            var outputSettings = new OutputSettings()
            {
                FileName = Path.ChangeExtension(inputSettings.FileName, "").TrimEnd('.')
            };
            await _outputWriter.Write(meshes, outputSettings);
        }
    }
}
