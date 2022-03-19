using McMaster.Extensions.CommandLineUtils;
using StrideGenerator.Data;

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

        _console.WriteLine($"Unique stride count: {meshObject.Strides.Distinct().Count()}");

        return meshObject;
    }
}
