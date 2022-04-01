using csFastFloat;
using Microsoft.Extensions.Logging;
using StrideGenerator.Data;
using System.Diagnostics;
using System.Globalization;
using Open.Text;
using System.Runtime.CompilerServices;

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
        //GC.TryStartNoGCRegion(100 * 1024 * 1024);

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
        var objectSpan = "object".AsSpan();

        Stopwatch sw = new();
        sw.Start();
        List<MeshObject> list = new List<MeshObject>();

        var lineNumber = 0;
        var currentObject = new MeshObject();
        string? currentMaterialName = null;

        foreach (var line in File.ReadLines(inputData.FileName))
        {
            var lineSpan = line.AsSpan();
            var verb = lineSpan.FirstSplit(' ', out var lineNextIndex);
            if (lineNextIndex == -1)
            {
                continue;
            }
            lineSpan = lineSpan.Slice(lineNextIndex);
            ReadOnlySpan<char> wordSpan;

            var str = verb.ToString();

            if (verb.SequenceEqual(vSpan) || verb.SequenceEqual(VSpan))
            {
                var arr = new double[3];
                for (var i = 0; i < 3; i++)
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    arr[i] = FastDoubleParser.ParseDouble(wordSpan);
                }
                vertices.Add(arr);
            }
            else if (verb.SequenceEqual(vnSpan) || verb.SequenceEqual(VNSpan))
            {
                var arr = new double[3];
                for (var i = 0; i < 3; i++)
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    arr[i] = FastDoubleParser.ParseDouble(wordSpan);
                }
                normals.Add(arr);
            }
            else if (verb.SequenceEqual(vtSpan) || verb.SequenceEqual(VTSpan))
            {
                var arr = new double[2];
                for (var i = 0; i < 2; i++)
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    if (i == 0)
                    {
                        arr[i] = FastDoubleParser.ParseDouble(wordSpan);
                    }
                    else if (i == 1)
                    {
                        arr[i] = 1.0d - FastDoubleParser.ParseDouble(wordSpan);
                    }
                }
                uvs.Add(arr);
            }
            else if (verb.SequenceEqual(fSpan) || verb.SequenceEqual(FSpan))
            {
                var strides = new Stride[3];
                for (var si = 0; si < 3; si++)
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    var faceSpan = wordSpan.FirstSplit('/', out var nextIndex);
                    var vertexIndex = int.Parse(faceSpan, NumberStyles.None, CultureInfo.InvariantCulture) - 1;

                    wordSpan = wordSpan.Slice(nextIndex);
                    faceSpan = wordSpan.FirstSplit('/', out nextIndex);
                    var uvIndex = int.Parse(faceSpan, NumberStyles.None, CultureInfo.InvariantCulture) - 1;

                    wordSpan = wordSpan.Slice(nextIndex);
                    var normalIndex = int.Parse(wordSpan, NumberStyles.None, CultureInfo.InvariantCulture) - 1;

                    strides[si] = new Stride
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
                lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                currentObject = new MeshObject()
                {
                    Name = wordSpan.ToString()
                };
                list.Add(currentObject);
                currentMaterialName = null;
            }
            else if (verb.SequenceEqual(commentSpan))
            {
                lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                if (wordSpan.SequenceEqual(objectSpan))
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    if(wordSpan.Length > 0)
                    {
                        currentObject = new MeshObject()
                        {
                            Name = wordSpan.ToString()
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

        //GC.EndNoGCRegion();

        return Task.FromResult(list.AsEnumerable());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<char> MoveToNextWord(ReadOnlySpan<char> lineSpan, out int lineNextIndex, out ReadOnlySpan<char> wordSpan)
    {
        do
        {
            wordSpan = lineSpan.FirstSplit(' ', out lineNextIndex);
            if (lineNextIndex == -1)
            {
                wordSpan = lineSpan;
                break;
            }
            else
            {
                lineSpan = lineSpan.Slice(lineNextIndex);
            }
        } while (wordSpan.Length == 0);
        return lineSpan;
    }
}
