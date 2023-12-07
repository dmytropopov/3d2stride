using StrideGenerator.Data;
using System.Diagnostics;

namespace StrideGenerator.Services;

public sealed class MeshOptimizer(IConsole console)
{
    private readonly IConsole _console = console;

    public MeshObject GetOptimized(MeshObject meshObject)
    {
        _console.WriteLine($"Mesh: '{meshObject.Name}'");
        _console.WriteLine($"Stride count: {meshObject.Strides.Count}");
        _console.WriteLine($"Face count: {meshObject.Faces.Count}");

        Stopwatch sw = new();
        sw.Start();

        SortedList<Stride, Stride> sorted = [];
        SortedSet<Stride> resorted = new(new StrideOriginalIndexComparer());
        foreach (var stride in meshObject.Strides)
        {
            var exists = sorted.TryGetValue(stride, out var existingSorted);
            if (exists)
            {
                stride.Face.Strides[stride.OriginalIndexInFace] = existingSorted!;
            }
            else
            {
                sorted.Add(stride, stride);
                resorted.Add(stride);
            }
            stride.Index = sorted.IndexOfKey(stride);
        }

        var i = 0;
        foreach (var stride in resorted)
        {
            stride.Index = i++;
        }

        var optimizedMesh = new MeshObject()
        {
            Name = meshObject.Name,
            Strides = resorted,
            Faces = meshObject.Faces
        };

        sw.Stop();
        _console.WriteLine($"Sorted stride count: {sorted.Count}");
        _console.WriteLine($"Sort Time: {sw.Elapsed}");

        return optimizedMesh;
    }
}
