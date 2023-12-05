using StrideGenerator.Data;

namespace StrideGenerator.Services;

public sealed class Generator(InputReaderFactory inputReaderFactory, IOutputWriter outputWriter, IConsole console) : IGenerator
{
    private readonly InputReaderFactory _inputReaderFactory = inputReaderFactory;
    private readonly IOutputWriter _outputWriter = outputWriter;
    private readonly IConsole _console = console;

    public async Task Generate(IEnumerable<InputSettings> inputs, OutputSettings outputSettings)
    {
        List<MeshObject> meshes = [];
        int inputIndex = 0;

        // TODO
        // When output is not specified and there is only 1 object in input, use input file name
        // If there are multiple objects in input file, write them all and use object names as output file names
        foreach (var inputSettings in inputs)
        {
            var stridePiecesForOneInputFile = outputSettings.OutputAttributes.StrideMap.Where(w => w.InputIndex == inputIndex).ToList();

            if (stridePiecesForOneInputFile.Count != 0)
            {
                var reader = _inputReaderFactory.GetReader(inputSettings.FileFormat);
                await reader.ReadInput(meshes, inputSettings, stridePiecesForOneInputFile, outputSettings.MergeObjects, outputSettings.OutputAttributes.GetStrideSize());
            }
            else
            {
                _console.WriteLine($"Input {inputIndex} has no attributes referenced in stride map, skipping.");
            }

            inputIndex++;
        }
        await _outputWriter.Write(meshes, inputs, outputSettings);
    }
}
