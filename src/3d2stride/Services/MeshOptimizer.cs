using McMaster.Extensions.CommandLineUtils;
using StrideGenerator.Data;
using System.Collections;
using System.Collections.Generic;
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
        //var uniqueStrides = meshObject.Strides.Distinct().ToList();
        //var optimizedMesh = new MeshObject()
        //{
        //    Name = meshObject.Name,
        //    Strides = uniqueStrides
        //};

        //_console.WriteLine($"Unique stride count: {optimizedMesh.Strides.Count()}");
        sw.Stop();
        _console.WriteLine($"Time: {sw.Elapsed}");

        sw.Restart();
        SortedList<Stride, Stride> sorted = new();
        //Dictionary<Stride, Stride> sortedMap = new();
        //var index = 0;
        foreach (var stride in meshObject.Strides)
        {
            //var indexOfKey = sorted.IndexOfKey(stride);
            var exist = sorted.TryGetValue(stride, out var existingSorted);
            if (exist)
            {
                stride.Face.Strides[stride.OriginalIndexInFace] = existingSorted;
            }
            else
            {
                sorted.Add(stride, stride);
            }
        }
        //var resorted = sorted.OrderBy(s => s.Value).Select(s => s.Key).ToList();

        //var optimizedFaces = new List<Face>();
        //foreach (var face in meshObject.Faces)
        //{
        //    var _face = new Face { MaterialName = face.MaterialName };
        //    foreach (var faceStride in face.Strides)
        //    {
        //        //_face.Strides.Add
        //    }
        //}

        var optimizedMesh = new MeshObject()
        {
            Name = meshObject.Name,
            Strides = sorted.Select(s => s.Value).ToList(),
            Faces = meshObject.Faces
        };

        sw.Stop();
        _console.WriteLine($"Sorted stride count: {sorted.Count()}");
        _console.WriteLine($"Sort Time: {sw.Elapsed}");

        //foreach (var face in meshObject.Faces)
        //{
        //    optimizedMesh.Faces.Add(new Face
        //    {
        //        MaterialName = face.MaterialName,
        //        Indices = face.Indices.Select(i => optimizedMesh.Strides.IndexOf(optimizedMesh.Strides.Single(o => ReferenceEquals(o, meshObject.Strides.ElementAt(i))))).ToList()
        //    });
        //}

        return optimizedMesh;
    }
}
