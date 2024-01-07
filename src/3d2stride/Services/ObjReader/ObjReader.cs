﻿using csFastFloat;
using StrideGenerator.Data;
using System.Diagnostics;
using System.Globalization;
using Open.Text;
using System.Runtime.CompilerServices;
using System;

namespace StrideGenerator.Services.Obj;

public sealed class ObjReader(IConsole console) : IInputReader
{
    private readonly IConsole _console = console;
    private List<float[]> vertices = null!;
    private List<float[]> normals = null!;
    private List<float[]> uvs = null!;
    private bool _readNormals;
    private bool _readVertices;
    private bool _readUVs;
    private int _attributesCount;
    private bool _meshesExist;

    public Task ReadInput(List<MeshObject> meshes, InputSettings inputData, StridePiece[] stridePieces, ProcessingPiece[] processingPieces, bool mergeObjects, int strideSize)
    {
        vertices = new(65536);
        normals = new(65536);
        uvs = new(65536);

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

        var allAttributes = stridePieces.SelectMany(s => s.AttributeTypes, (stridePiece, attributeType) => new { stridePiece, attributeType }).ToArray();
        _readVertices = allAttributes.Any(x => x.attributeType == AttributeComponentType.VertexX || x.attributeType == AttributeComponentType.VertexY || x.attributeType == AttributeComponentType.VertexZ);
        _readUVs = allAttributes.Any(x => x.attributeType == AttributeComponentType.TextureCoordU || x.attributeType == AttributeComponentType.TextureCoordV);
        _readNormals = allAttributes.Any(x => x.attributeType == AttributeComponentType.NormalX || x.attributeType == AttributeComponentType.NormalY || x.attributeType == AttributeComponentType.NormalZ);
        _attributesCount = allAttributes.Length;

        var vertexProcessing = processingPieces.Where(w => w.AttributeType == AttributeType.Vertex).ToArray();
        var normalProcessing = processingPieces.Where(w => w.AttributeType == AttributeType.Normal).ToArray();
        var uvProcessing = processingPieces.Where(w => w.AttributeType == AttributeType.TextureCoordinate).ToArray();

        bool processVertices = vertexProcessing.Length > 0;
        bool processNormals = normalProcessing.Length > 0;
        bool processUVs = uvProcessing.Length > 0;

        _meshesExist = meshes.Count > 0;

        Stopwatch sw = new();
        sw.Start();

        var lineNumber = 0;
        var currentObject = meshes.FirstOrDefault() ?? new MeshObject();
        string? currentMaterialName = null;
        int currentFaceIndex = 0; // TODO: check for identical face count in secondary inputs

        foreach (var line in File.ReadLines(inputData.FileName))
        {
            var lineSpan = line.AsSpan();
            var verb = lineSpan.FirstSplit(' ', out var lineNextIndex);
            if (lineNextIndex == -1)
            {
                continue;
            }
            lineSpan = lineSpan[lineNextIndex..];
            ReadOnlySpan<char> wordSpan;

            if (_readVertices && (verb.SequenceEqual(vSpan) || verb.SequenceEqual(VSpan)))
            {
                var arr = new float[3];
                for (var i = 0; i < 3; i++)
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    arr[i] = (float)FastDoubleParser.ParseDouble(wordSpan);
                }

                if (processVertices)
                {
                    StrideDataProcessor.Process(arr, vertexProcessing);
                }
                vertices.Add(arr);
            }
            else if (_readNormals && (verb.SequenceEqual(vnSpan) || verb.SequenceEqual(VNSpan)))
            {
                var arr = new float[3];
                for (var i = 0; i < 3; i++)
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    arr[i] = (float)FastDoubleParser.ParseDouble(wordSpan);
                }

                if (processNormals)
                {
                    StrideDataProcessor.Process(arr, normalProcessing);
                }
                normals.Add(arr);
            }
            else if (_readUVs && (verb.SequenceEqual(vtSpan) || verb.SequenceEqual(VTSpan)))
            {
                var arr = new float[2];
                for (var i = 0; i < 2; i++)
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    arr[i] = (float)FastDoubleParser.ParseDouble(wordSpan);
                }

                if (processUVs)
                {
                    StrideDataProcessor.Process(arr, uvProcessing);
                }
                uvs.Add(arr);
            }
            else if (verb.SequenceEqual(fSpan) || verb.SequenceEqual(FSpan))
            {
                Stride[] strides;

                if (!_meshesExist)
                {
                    strides = [new(strideSize), new(strideSize), new(strideSize)];

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
                    currentObject.Strides.Add(strides[0]);
                    currentObject.Strides.Add(strides[1]);
                    currentObject.Strides.Add(strides[2]);
                    currentObject.Faces.Add(face);
                }
                else
                {
                    strides = currentObject.Faces[currentFaceIndex].Strides;
                    currentFaceIndex++;
                }

                for (var si = 0; si < 3; si++)
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    var faceSpan = wordSpan.FirstSplit('/', out var nextIndex);
                    var vertexIndex = int.Parse(faceSpan, NumberStyles.None, CultureInfo.InvariantCulture) - 1;

                    int uvIndex = -1;
                    if (nextIndex > -1)
                    {
                        wordSpan = wordSpan[nextIndex..];
                        faceSpan = wordSpan.FirstSplit('/', out nextIndex);
                        int.TryParse(faceSpan, NumberStyles.None, CultureInfo.InvariantCulture, out uvIndex);
                        uvIndex--;
                    }

                    int normalIndex = -1;
                    if (nextIndex > -1)
                    {
                        wordSpan = wordSpan[nextIndex..];
                        int.TryParse(wordSpan, NumberStyles.None, CultureInfo.InvariantCulture, out normalIndex);
                        normalIndex--;

                        if (processNormals)
                        {
                            StrideDataProcessor.Process(normals[normalIndex], normalProcessing);
                        }
                    }

                    var stride = strides[si];
                    unsafe
                    {
                        fixed (void* ptr = &stride.Data[0])
                        {
                            for (int attributeIndex = 0; attributeIndex < _attributesCount; attributeIndex++)
                            {
                                byte* bytePtr = (byte*)ptr + allAttributes[attributeIndex].stridePiece.Offset;

                                if (allAttributes[attributeIndex].attributeType == AttributeComponentType.VertexX)
                                {
                                    stride.WriteInFormat(vertices[vertexIndex][0], bytePtr, allAttributes[attributeIndex].stridePiece.Format);
                                }
                                if (allAttributes[attributeIndex].attributeType == AttributeComponentType.VertexY)
                                {
                                    stride.WriteInFormat(vertices[vertexIndex][1], bytePtr, allAttributes[attributeIndex].stridePiece.Format);
                                }
                                if (allAttributes[attributeIndex].attributeType == AttributeComponentType.VertexZ)
                                {
                                    stride.WriteInFormat(vertices[vertexIndex][2], bytePtr, allAttributes[attributeIndex].stridePiece.Format);
                                }
                                if (allAttributes[attributeIndex].attributeType == AttributeComponentType.TextureCoordU)
                                {
                                    stride.WriteInFormat(uvs[uvIndex][0], bytePtr, allAttributes[attributeIndex].stridePiece.Format);
                                }
                                if (allAttributes[attributeIndex].attributeType == AttributeComponentType.TextureCoordV)
                                {
                                    stride.WriteInFormat(uvs[uvIndex][1], bytePtr, allAttributes[attributeIndex].stridePiece.Format);
                                }
                                if (allAttributes[attributeIndex].attributeType == AttributeComponentType.NormalX)
                                {
                                    stride.WriteInFormat(normals[normalIndex][0], bytePtr, allAttributes[attributeIndex].stridePiece.Format);
                                }
                                if (allAttributes[attributeIndex].attributeType == AttributeComponentType.NormalY)
                                {
                                    stride.WriteInFormat(normals[normalIndex][1], bytePtr, allAttributes[attributeIndex].stridePiece.Format);
                                }
                                if (allAttributes[attributeIndex].attributeType == AttributeComponentType.NormalZ)
                                {
                                    stride.WriteInFormat(normals[normalIndex][2], bytePtr, allAttributes[attributeIndex].stridePiece.Format);
                                }
                            }
                        }
                    }
                }
            }
            else if (verb.SequenceEqual(oSpan) || verb.SequenceEqual(OSpan))
            {
                if (!mergeObjects) // ignore objects when in merge mode
                {
                    lineSpan = MoveToNextWord(lineSpan, out lineNextIndex, out wordSpan);
                    currentObject = new MeshObject()
                    {
                        Name = wordSpan.ToString()
                    };
                    meshes.Add(currentObject);
                    currentMaterialName = null;
                }
            }
            else if (verb.SequenceEqual(commentSpan))
            {
                if (!mergeObjects) // ignore objects when in merge mode
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
                            meshes.Add(currentObject);
                            currentMaterialName = null;
                            _console.WriteLine($"Reading object {currentObject.Name}");
                        }
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
        if (meshes.Count == 0)
        {
            meshes.Add(currentObject);
        }

        sw.Stop();
        _console.WriteLine($"Read time: {sw.Elapsed}");
        return Task.CompletedTask;
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
                lineSpan = lineSpan[lineNextIndex..];
            }
        } while (wordSpan.Length == 0);
        return lineSpan;
    }
}
