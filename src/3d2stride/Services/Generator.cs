namespace StrideGenerator.Services;

public sealed class Generator : IGenerator
{
    private readonly InputReaderFactory _inputReaderFactory;
    private readonly IOutputWriter _outputWriter;

    public Generator(InputReaderFactory inputReaderFactory, IOutputWriter outputWriter)
    {
        _inputReaderFactory = inputReaderFactory;
        _outputWriter = outputWriter;
    }

    public async Task Generate(IEnumerable<InputSettings> inputs, OutputSettings outputSettings)
    {
        // TODO
        // When output is not specified and there is only 1 object in input, use input file name
        // If there are multiple objects in input file, write them all and use object names as output file names
        foreach (var inputSettings in inputs)
        {
            var reader = _inputReaderFactory.GetReader(inputSettings.FileFormat);
            var meshes = await reader.ReadInput(inputSettings, outputSettings);
            
            await _outputWriter.Write(meshes, inputs, outputSettings);
        }
    }
}
