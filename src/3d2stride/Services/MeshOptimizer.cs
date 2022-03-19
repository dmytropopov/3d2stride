using McMaster.Extensions.CommandLineUtils;
using StrideGenerator.Data;
using System.Diagnostics;

namespace StrideGenerator.Services;

public class MeshOptimizer
{
    private readonly IConsole _console;

    public MeshOptimizer(IConsole console)
    {
        _console = console;
    }

    public MeshObject GetOptimized(MeshObject meshObject)
    {
        _console.WriteLine($"Mesh: '{meshObject.Name}'");
        _console.WriteLine($"Stride count: {meshObject.Strides.Count()}");
        _console.WriteLine($"Face count: {meshObject.Faces.Count()}");

        Stopwatch sw = new();
        sw.Start();

        sw.Stop();
        _console.WriteLine($"Time: {sw.Elapsed}");

        sw.Restart();
        SortedList<Stride, Stride> sorted = new();
        SortedList<Stride, Stride> resorted = new(new StrideOriginalIndexComparer());
        foreach (var stride in meshObject.Strides)
        {
            var exists = sorted.TryGetValue(stride, out var existingSorted);
            if (exists)
            {
                stride.Face.Strides[stride.OriginalIndexInFace] = existingSorted;
            }
            else
            {
                sorted.Add(stride, stride);
                resorted.Add(stride, stride);
            }
            stride.Index = sorted.IndexOfKey(stride);
        }

        foreach (var stride in resorted)
        {
            stride.Value.Index = resorted.IndexOfKey(stride.Value);
        }

        var optimizedMesh = new MeshObject()
        {
            Name = meshObject.Name,
            Strides = resorted.Select(s=>s.Value).ToList(),
            Faces = meshObject.Faces
        };

        sw.Stop();
        _console.WriteLine($"Sorted stride count: {sorted.Count()}");
        _console.WriteLine($"Sort Time: {sw.Elapsed}");

        return optimizedMesh;
    }
}
