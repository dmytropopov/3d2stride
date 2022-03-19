using Microsoft.Extensions.Logging;
using StrideGenerator.Data;
using System.Globalization;

namespace StrideGenerator.Services.Obj;

public class ObjReader : IInputReader
{
    private readonly ILogger<ObjReader> _logger;
    private readonly List<double[]> vertices = new();
    private readonly List<double[]> normals = new();
    private readonly List<double[]> uvs = new();

    public ObjReader(ILogger<ObjReader> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<MeshObject>> ReadInput(InputData inputData)
    {
        using var reader = new StreamReader(inputData.FileName);

        List<MeshObject> list = new List<MeshObject>();

        string? line;
        var lineNumber = 0;
        var currentObject = new MeshObject();
        string? currentMaterialName = null;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            var words = line.Trim().Split(" ");
            var verb = words[0].ToUpperInvariant();
            switch (verb)
            {
                case "":
                    break;
                case "#":
                    break;
                case "V":
                    vertices.Add(new double[] {
                            double.Parse(words[1], CultureInfo.InvariantCulture), // swap Y and Z
                            double.Parse(words[3], CultureInfo.InvariantCulture),
                            double.Parse(words[2], CultureInfo.InvariantCulture)
                        });
                    break;
                case "VN":
                    normals.Add(new double[] {
                            double.Parse(words[1], CultureInfo.InvariantCulture), // swap Y and Z
                            double.Parse(words[3], CultureInfo.InvariantCulture),
                            double.Parse(words[2], CultureInfo.InvariantCulture)
                        });
                    break;
                case "VT":
                    uvs.Add(new double[] {
                            double.Parse(words[1], CultureInfo.InvariantCulture),
                            double.Parse(words[2], CultureInfo.InvariantCulture)
                        });
                    break;
                case "O":
                    currentObject = new MeshObject()
                    {
                        Name = words[1]
                    };
                    list.Add(currentObject);
                    currentMaterialName = null;
                    break;
                case "F":
                    var faces = words.Skip(1)
                        .Select(faceVertexString =>
                        {
                            var faceIndices = faceVertexString.Split("/");
                            return new
                            {
                                vertexIndex = int.Parse(faceIndices[0], CultureInfo.InvariantCulture) - 1,
                                uvIndex = int.Parse(faceIndices[1], CultureInfo.InvariantCulture) - 1,
                                normalIndex = int.Parse(faceIndices[2], CultureInfo.InvariantCulture) - 1
                            };
                        });
                    if (faces.Any(f => f.normalIndex != faces.First().normalIndex))
                    {
                        _logger.LogWarning("Different normals in line {line}", lineNumber);
                    }
                    var face = new Face()
                    {
                        MaterialName = currentMaterialName,
                        FaceVertices = faces.Select(s => new FaceVertex
                        {
                            Vertex = vertices[s.vertexIndex],
                            Normal = normals[s.normalIndex],
                            Uv = uvs[s.uvIndex]
                        }).ToList()
                    };

                    // reverse clockwiseness of the vertices after normal flip
                    var reversedTail = face.FaceVertices.Skip(1).Reverse();
                    var reversedAll = new List<FaceVertex>
                        {
                            face.FaceVertices[0]
                        };
                    reversedAll.AddRange(reversedTail);
                    face.FaceVertices = reversedAll;

                    currentObject.Faces.Add(face);
                    break;
                case "MTLLIB":
                    break;
                case "USEMTL":
                    currentMaterialName = words[1];
                    break;
                default:
                    _logger.LogInformation("Line {line}. Unknown verb: {verb}", lineNumber, verb);
                    break;
            }

            lineNumber++;
        }

        return list;
    }
}
