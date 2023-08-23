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
    private readonly List<float[]> vertices = new(65536);
    private readonly List<float[]> normals = new(65536);
    private readonly List<float[]> uvs = new(65536);

    public ObjReader(ILogger<ObjReader> logger)
    {
        _logger = logger;
    }

    private bool _readNormals;
    private bool _readUVs;
    private int _attributesCount;

    public Task<IEnumerable<MeshObject>> ReadInput(InputSettings inputData, OutputSettings outputSettings)
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
        var objectSpan = "object".AsSpan();

        int strideSize = outputSettings.OutputAttributes.GetStrideSize();
        _readNormals = outputSettings.OutputAttributes.Attributes.Any(x => x.AttributeInfo.AttributeType == AttributeType.Normal);
        _readUVs = outputSettings.OutputAttributes.Attributes.Any(x => x.AttributeInfo.AttributeType == AttributeType.TextureCoords
            || x.AttributeInfo.AttributeType == AttributeType.TextureCoordU
            || x.AttributeInfo.AttributeType == AttributeType.TextureCoordV);
        _attributesCount = outputSettings.OutputAttributes.Attributes.Count;

        if (outputSettings.OutputAttributes.Attributes.Any(x => x.Index > 0))
        {
            throw new Exception("Reading attributes with index > 0 is not supported (yet).");
        }
        //if (outputSettings.OutputAttributes.Attributes.Any(x => x.Format == AttributeFormat.HalfFloat))
        //{
        //    throw new Exception("Half-float attributes are not supported (yet).");
        //}

        Stopwatch sw = new();
        sw.Start();
        List<MeshObject> objectsList = new();

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

            if (verb.SequenceEqual(vSpan) || verb.SequenceEqual(VSpan))
            {
                var arr = new float[3];
                for (var i = 0; i < 3; i++)
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    arr[i] = (float)FastDoubleParser.ParseDouble(wordSpan);
                }
                vertices.Add(arr);
            }
            else if (verb.SequenceEqual(vnSpan) || verb.SequenceEqual(VNSpan))
            {
                if (_readNormals)
                {
                    var arr = new float[3];
                    for (var i = 0; i < 3; i++)
                    {
                        lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                        arr[i] = (float)FastDoubleParser.ParseDouble(wordSpan);
                    }
                    normals.Add(arr);
                }
            }
            else if (verb.SequenceEqual(vtSpan) || verb.SequenceEqual(VTSpan))
            {
                if (_readUVs)
                {
                    var arr = new float[2];
                    for (var i = 0; i < 2; i++)
                    {
                        lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                        if (i == 0)
                        {
                            arr[i] = (float)FastDoubleParser.ParseDouble(wordSpan);
                        }
                        else if (i == 1)
                        {
                            arr[i] = 1.0f - (float)FastDoubleParser.ParseDouble(wordSpan);
                        }
                    }
                    uvs.Add(arr);
                }
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

                    var stride = new Stride(strideSize);
                    strides[si] = stride;
                    unsafe
                    {
                        fixed (void* ptr = &stride.Data[0])
                        {
                            byte* bytePtr = (byte*)ptr;
                            for (int attributeIndex = 0; attributeIndex < _attributesCount; attributeIndex++)
                            {
                                if (outputSettings.OutputAttributes.Attributes[attributeIndex].AttributeInfo.AttributeType == AttributeType.Vertex)
                                {
                                    if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.Float)
                                    {
                                        writeFloat(vertices[vertexIndex][0], ref bytePtr);
                                        writeFloat(vertices[vertexIndex][1], ref bytePtr);
                                        writeFloat(vertices[vertexIndex][2], ref bytePtr);
                                    }
                                    else if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.HalfFloat)
                                    {
                                        writeHalfFloat(vertices[vertexIndex][0], ref bytePtr);
                                        writeHalfFloat(vertices[vertexIndex][1], ref bytePtr);
                                        writeHalfFloat(vertices[vertexIndex][2], ref bytePtr);
                                    }
                                }
                                if (outputSettings.OutputAttributes.Attributes[attributeIndex].AttributeInfo.AttributeType == AttributeType.TextureCoords)
                                {
                                    if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.Float)
                                    {
                                        writeFloat(uvs[uvIndex][0], ref bytePtr);
                                        writeFloat(uvs[uvIndex][1], ref bytePtr);
                                    }
                                    else if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.HalfFloat)
                                    {
                                        writeHalfFloat(uvs[uvIndex][0], ref bytePtr);
                                        writeHalfFloat(uvs[uvIndex][1], ref bytePtr);
                                    }
                                }
                                if (outputSettings.OutputAttributes.Attributes[attributeIndex].AttributeInfo.AttributeType == AttributeType.TextureCoordU)
                                {
                                    if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.Float)
                                    {
                                        writeFloat(uvs[uvIndex][0], ref bytePtr);
                                    }
                                    else if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.HalfFloat)
                                    {
                                        writeHalfFloat(uvs[uvIndex][0], ref bytePtr);
                                    }
                                }
                                if (outputSettings.OutputAttributes.Attributes[attributeIndex].AttributeInfo.AttributeType == AttributeType.TextureCoordV)
                                {
                                    if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.Float)
                                    {
                                        writeFloat(uvs[uvIndex][1], ref bytePtr);
                                    }
                                    else if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.HalfFloat)
                                    {
                                        writeHalfFloat(uvs[uvIndex][1], ref bytePtr);
                                    }
                                }
                                if (outputSettings.OutputAttributes.Attributes[attributeIndex].AttributeInfo.AttributeType == AttributeType.Normal)
                                {
                                    if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.Float)
                                    {
                                        writeFloat(normals[normalIndex][0], ref bytePtr);
                                        writeFloat(normals[normalIndex][1], ref bytePtr);
                                        writeFloat(normals[normalIndex][2], ref bytePtr);
                                    }
                                    else if (outputSettings.OutputAttributes.Attributes[attributeIndex].Format == AttributeFormat.HalfFloat)
                                    {
                                        writeHalfFloat(normals[normalIndex][0], ref bytePtr);
                                        writeHalfFloat(normals[normalIndex][1], ref bytePtr);
                                        writeHalfFloat(normals[normalIndex][2], ref bytePtr);
                                    }
                                }
                            }
                        }
                    }
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
                    s.OriginalIndex = currentObject.Strides.Count + i++;
                }

                currentObject.Strides.AddRange(strides);
                currentObject.Faces.Add(face);
            }
            else if (verb.SequenceEqual(oSpan) || verb.SequenceEqual(OSpan))
            {
                if (!outputSettings.MergeObjects) // ignore objects when in merge mode
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    currentObject = new MeshObject()
                    {
                        Name = wordSpan.ToString()
                    };
                    objectsList.Add(currentObject);
                    currentMaterialName = null;
                }
            }
            else if (verb.SequenceEqual(commentSpan))
            {
                lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                if (wordSpan.SequenceEqual(objectSpan))
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    if (wordSpan.Length > 0)
                    {
                        currentObject = new MeshObject()
                        {
                            Name = wordSpan.ToString()
                        };
                        objectsList.Add(currentObject);
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

        // Obj file had no objects defined - add current object
        if (objectsList.Count == 0)
        {
            objectsList.Add(currentObject);
        }

        sw.Stop();
        Console.WriteLine($"Read time: {sw.Elapsed}");

        return Task.FromResult(objectsList.AsEnumerable());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void writeFloat(float data, ref byte* bytePtr)
    {
        *(float*)bytePtr = data;
        bytePtr += sizeof(float);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void writeHalfFloat(float data, ref byte* bytePtr)
    {
        *(Half*)bytePtr = (Half)data;
        bytePtr += sizeof(float);
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
