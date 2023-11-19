using StrideGenerator.Data;
using System.Diagnostics;

namespace StrideGenerator.Services;

public sealed class OutputWriter : IOutputWriter
{
    private readonly IConsole _console;
    private readonly MeshOptimizer _meshOptimizer;

    public OutputWriter(IConsole console, MeshOptimizer meshOptimizer)
    {
        _console = console;
        _meshOptimizer = meshOptimizer;
    }

    public Task Write(IEnumerable<MeshObject> meshes, IEnumerable<InputSettings> inputs, OutputSettings outputSettings)
    {
        var inputFileName = Path.ChangeExtension(inputs.First().FileName, "").TrimEnd('.');
        if (meshes.Count() > 1 && !(outputSettings.FileName?.Contains('{') ?? false))
        {
            _console.WriteLine("Multiple meshes found, but output does not contain template like {0} or {1} - using default template.");
            outputSettings.FileName = inputFileName + "{1}";
        }
        else if (meshes.Count() == 1 && string.IsNullOrEmpty(outputSettings.FileName))
        {
            _console.WriteLine($"One mesh found, but output is not set, using output name '{inputFileName}'.");
            outputSettings.FileName = inputFileName;
        }

        var sw = Stopwatch.StartNew();
        var alignmentPadding = outputSettings.Alignment != 0
            ? new byte[outputSettings.OutputAttributes.GetStrideSize() > outputSettings.Alignment
                ? outputSettings.OutputAttributes.GetStrideSize() % outputSettings.Alignment
                : outputSettings.Alignment % outputSettings.OutputAttributes.GetStrideSize()]
            : new byte[0];

        int i = 0;
        foreach (var optimized in meshes.Select(_meshOptimizer.GetOptimized))
        {
            var fileName = GetFileName(outputSettings, i++, optimized.Name);
            _console.WriteLine($"Writing object {optimized.Name} to file {fileName}");

            using var stridesStream = File.Open(Path.ChangeExtension(fileName + "-strides", "bin"), FileMode.Create);
            using var stridesWriter = new BinaryWriter(stridesStream);
            using var indicesStream = File.Open(Path.ChangeExtension(fileName + "-indices", "bin"), FileMode.Create);
            using var indicesWriter = new BinaryWriter(indicesStream);

            foreach (var stride in optimized.Strides)
            {
                stridesWriter.Write(stride.Data);
                if (outputSettings.Alignment != 0)
                {
                    stridesWriter.Write(alignmentPadding);
                }
            }

            foreach (var face in optimized.Faces)
            {
                foreach (var stride in face.Strides)
                {
                    indicesWriter.Write((ushort)stride.Index);
                }
            }

            stridesWriter.Close();
            stridesStream.Close();
            indicesWriter.Close();
            indicesStream.Close();
        }
        sw.Stop();
        _console.WriteLine($"Write time: {sw.Elapsed}");

        return Task.CompletedTask;
    }

    private static string GetFileName(OutputSettings outputSettings, int index, string objectName)
        => string.Format(outputSettings.FileName, objectName, index);
}
