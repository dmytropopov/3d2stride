using csFastFloat;
using Microsoft.Extensions.Logging;
using StrideGenerator.Data;
using System.Diagnostics;
using System.Globalization;
using Open.Text;

namespace StrideGenerator.Services.Obj;

public sealed class ObjReader : IInputReader
{
    private readonly ILogger<ObjReader> _logger;
    private readonly List<double[]> vertices = new List<double[]>(65536);
    private readonly List<double[]> normals = new List<double[]>(65536);
    private readonly List<double[]> uvs = new List<double[]>(65536);

    public ObjReader(ILogger<ObjReader> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<MeshObject>> ReadInput(InputSettings inputData)
    {
        var commentSpan = "#".AsSpan();
        var vSpan = "v".AsSpan();
        var VSpan = "V".AsSpan();
        var vtSpan = "vt".AsSpan();
        var VTSpan = "VT".AsSpan();
        var vnSpan = "vn".AsSpan();
        var VNSpan = "VN".AsSpan();
        var oSpan = "o".AsSpan();
        var OSpan = "O".AsSpan();
        var fSpan = "f".AsSpan();
        var FSpan = "F".AsSpan();

        Stopwatch sw = new();
        sw.Start();
        List<MeshObject> list = new List<MeshObject>();

        var lineNumber = 0;
        var currentObject = new MeshObject();
        string? currentMaterialName = null;

        foreach (var line in File.ReadLines(inputData.FileName))
        {
            var words = line.SplitAsMemory(' ', StringSplitOptions.RemoveEmptyEntries);
            var verb = words.FirstOrDefault().Span;

            var str = verb.ToString();

            if (verb.SequenceEqual(vSpan) || verb.SequenceEqual(VSpan))
            {
                var i = 0;
                var arr = new double[3];
                foreach (var word in words.Skip(1))
                {
                    arr[i++] = FastDoubleParser.ParseDouble(word.Span);
                }
                vertices.Add(arr);
            }
            else if (verb.SequenceEqual(vnSpan) || verb.SequenceEqual(VNSpan))
            {
                var i = 0;
                var arr = new double[3];
                foreach (var word in words.Skip(1))
                {
                    arr[i++] = FastDoubleParser.ParseDouble(word.Span);
                }
                normals.Add(arr);
                //normals.Add(new double[] {
                //           FastDoubleParser.ParseDouble(words.ElementAt(1).Span),
                //           FastDoubleParser.ParseDouble(words.ElementAt(2).Span),
                //           FastDoubleParser.ParseDouble(words.ElementAt(3).Span)
                //        });
            }
            else if (verb.SequenceEqual(vtSpan) || verb.SequenceEqual(VTSpan))
            {
                var i = 0;
                var arr = new double[2];
                foreach (var word in words.Skip(1))
                {
                    if (i == 0)
                    {
                        arr[i++] = FastDoubleParser.ParseDouble(word.Span);
                    }
                    else if (i == 1)
                    {
                        arr[i++] = 1.0d - FastDoubleParser.ParseDouble(word.Span);
                    }
                }
                uvs.Add(arr);
                //uvs.Add(new double[] {
                //            FastDoubleParser.ParseDouble(words.ElementAt(1).Span),
                //            1.0d - FastDoubleParser.ParseDouble(words.ElementAt(2).Span)
                //        });
            }
            else if (verb.SequenceEqual(fSpan) || verb.SequenceEqual(FSpan))
            {
                var strides = new Stride[3];
                int si = 0;
                foreach (var word in words.Skip(1))
                {
                    var windowSpan = word.Span;
                    var faceSpan = windowSpan.FirstSplit('/', out var nextIndex);
                    var vertexIndex = int.Parse(faceSpan, NumberStyles.None, CultureInfo.InvariantCulture) - 1;
                    
                    windowSpan = windowSpan.Slice(nextIndex);
                    faceSpan = windowSpan.FirstSplit('/', out nextIndex);
                    var uvIndex = int.Parse(faceSpan, NumberStyles.None, CultureInfo.InvariantCulture) - 1;

                    windowSpan = windowSpan.Slice(nextIndex);
                    var normalIndex = int.Parse(windowSpan, NumberStyles.None, CultureInfo.InvariantCulture) - 1;

                    strides[si++] = new Stride
                    {
                        Coordinates = vertices.ElementAt(vertexIndex),
                        Uvs = uvs.ElementAt(uvIndex),
                        Normals = normals.ElementAt(normalIndex)
                    };
                }

                var face = new Face()
                {
                    MaterialName = currentMaterialName,
                    Strides = strides
                };
                var i = 0;
                foreach (var s in strides)
                {
                    s.Face = face;
                    s.OriginalIndexInFace = Array.IndexOf(face.Strides, s);
                    s.OriginalIndex = currentObject.Strides.Count() + i++;
                }

                currentObject.Strides.AddRange(strides);
                currentObject.Faces.Add(face);
            }
            else if (verb.SequenceEqual(oSpan) || verb.SequenceEqual(OSpan))
            {
                currentObject = new MeshObject()
                {
                    Name = words.ElementAt(1).ToString(),
                };
                list.Add(currentObject);
                currentMaterialName = null;
            }
            else if (verb.SequenceEqual(commentSpan))
            {
                if (words.Count() == 3)
                {
                    // strange 3ds max OBJ output is not using 'O' verb
                    if (words.ElementAt(1).Span.Equals("object", StringComparison.InvariantCultureIgnoreCase))
                    {
                        currentObject = new MeshObject()
                        {
                            Name = words.ElementAt(2).ToString()
                        };
                        list.Add(currentObject);
                        currentMaterialName = null;
                        Console.WriteLine($"Reading object {currentObject.Name}");
                    }
                }
            }
            //case "MTLLIB":
            //    break;
            //case "USEMTL":
            //    currentMaterialName = words[1];
            //    break;
            //case "G":
            //    currentObject.Name = words[1];
            //    break;
            //case "S":
            //    break;
            //default:
            //    _logger.LogInformation("Line {line}. Unknown verb: {verb}", lineNumber, verb);
            //    break;
            //}

            lineNumber++;
        }
        sw.Stop();
        Console.WriteLine($"Read time: {sw.Elapsed}");

        return Task.FromResult(list.AsEnumerable());
    }
}
