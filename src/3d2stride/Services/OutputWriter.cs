using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using StrideGenerator.Data;
using System.Diagnostics;

namespace StrideGenerator.Services;

public class OutputWriter : IOutputWriter
{
    private readonly IConsole _console;
    private readonly ILogger<OutputWriter> _logger;
    private readonly MeshOptimizer _meshOptimizer;

    public OutputWriter(ILogger<OutputWriter> logger, IConsole console, MeshOptimizer meshOptimizer)
    {
        _console = console;
        _logger = logger;
        _meshOptimizer = meshOptimizer;
    }

    public Task Write(IEnumerable<MeshObject> meshes, IEnumerable<InputSettings> inputs, OutputSettings outputSettings)
    {
        if (meshes.Count() > 0 && !(outputSettings.FileName?.Contains("{") ?? false))
        {
            _console.WriteLine("Multiple meshes found, but output does not contain template like {0} or {1} - using default template.");
            var inputFileName = Path.ChangeExtension(inputs.First().FileName, "").TrimEnd('.');
            outputSettings.FileName = inputFileName + "{1}";
        }

        var sw = Stopwatch.StartNew();

        int i = 0;
        foreach (var optimized in meshes.Select(m => _meshOptimizer.GetOptimized(m)))
        {
            var fileName = GetFileName(outputSettings, i++, optimized.Name);
            Console.WriteLine($"Writing object {optimized.Name} to file {fileName}");

            using var stridesStream = File.Open(Path.ChangeExtension(fileName + "-strides", "bin"), FileMode.Create);
            using var stridesWriter = new BinaryWriter(stridesStream);
            using var indicesStream = File.Open(Path.ChangeExtension(fileName + "-indices", "bin"), FileMode.Create);
            using var indicesWriter = new BinaryWriter(indicesStream);

            foreach (var stride in optimized.Strides)
            {
                stridesWriter.Write((float)stride.Coordinates[0]);
                stridesWriter.Write((float)stride.Coordinates[1]);
                stridesWriter.Write((float)stride.Coordinates[2]);
                stridesWriter.Write((float)stride.Uvs[0]);
                stridesWriter.Write((float)stride.Uvs[1]);
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
        Console.WriteLine($"Write time: {sw.Elapsed}");

        return Task.CompletedTask;
    }

    private static string GetFileName(OutputSettings outputSettings, int index, string objectName)
    {
        var result = string.Format(outputSettings.FileName, objectName, index);

        return result;
    }
}
