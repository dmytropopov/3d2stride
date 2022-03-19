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

    public Task Write(IEnumerable<MeshObject> meshes, OutputSettings outputSettings)
    {
        using var stridesStream = File.Open(Path.ChangeExtension(outputSettings.FileName + "-strides", "bin"), FileMode.Create);
        using var stridesWriter = new BinaryWriter(stridesStream);
        using var indicesStream = File.Open(Path.ChangeExtension(outputSettings.FileName + "-indices", "bin"), FileMode.Create);
        using var indicesWriter = new BinaryWriter(indicesStream);

        var sw = Stopwatch.StartNew();

        foreach (var optimized in meshes.Select(m=>_meshOptimizer.GetOptimized(m)))
        {
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
                    indicesWriter.Write((ushort)optimized.Strides.IndexOf(stride));
                }
                //foreach (var index in face.Indices)
                //{
                //    var stride = optimized.Strides.ElementAt(index);
                //    stridesWriter.Write((float)stride.Coordinates[0]);
                //    stridesWriter.Write((float)stride.Coordinates[1]);
                //    stridesWriter.Write((float)stride.Coordinates[2]);
                //    stridesWriter.Write((float)stride.Uvs[0]);
                //    stridesWriter.Write((float)stride.Uvs[1]);

                //    indicesWriter.Write((ushort)index);
                //}
            }
        }
        sw.Stop();
        Console.WriteLine($"Write time: {sw.Elapsed}");
        stridesWriter.Close();
        stridesStream.Close();
        indicesWriter.Close();
        indicesStream.Close();

        return Task.CompletedTask;
    }
}
