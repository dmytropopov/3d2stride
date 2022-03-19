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

    public async Task<IEnumerable<MeshObject>> ReadInput(InputSettings inputData)
    {
        using var reader = new StreamReader(inputData.FileName);

        List<MeshObject> list = new List<MeshObject>();

        string? line;
        var lineNumber = 0;
        var currentObject = new MeshObject();
        string? currentMaterialName = null;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            var words = line.Trim().Split(" ")
                .Where(w => !string.IsNullOrEmpty(w)).ToArray(); // skip empty words (in case of double spaces etc)

            if (words.Length == 0)
            {
                continue;
            }

            var verb = words[0].ToUpperInvariant();
            switch (verb)
            {
                case "":
                    break;
                case "#":
                    if (words.Length == 3)
                    {
                        // strange 3ds max OBJ output is not using 'O' verb
                        if (words[1].ToLowerInvariant() == "object")
                        {
                            currentObject = new MeshObject()
                            {
                                Name = words[2]
                            };
                            list.Add(currentObject);
                            currentMaterialName = null;
                        }
                    }
                    break;
                case "V":
                    vertices.Add(new double[] {
                            double.Parse(words[1], CultureInfo.InvariantCulture),
                            double.Parse(words[2], CultureInfo.InvariantCulture),
                            double.Parse(words[3], CultureInfo.InvariantCulture)
                        });
                    break;
                case "VN":
                    normals.Add(new double[] {
                            double.Parse(words[1], CultureInfo.InvariantCulture),
                            double.Parse(words[2], CultureInfo.InvariantCulture),
                            double.Parse(words[3], CultureInfo.InvariantCulture)
                        });
                    break;
                case "VT":
                    uvs.Add(new double[] {
                            double.Parse(words[1], CultureInfo.InvariantCulture),
                            1.0d - double.Parse(words[2], CultureInfo.InvariantCulture)
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
                    var strides = words.Skip(1)
                        .Select(faceVertexString =>
                        {
                            var faceIndices = faceVertexString.Split("/");
                            return new Stride
                            {
                                Coordinates = vertices.ElementAt(int.Parse(faceIndices[0], CultureInfo.InvariantCulture) - 1),
                                Uvs = uvs.ElementAt(int.Parse(faceIndices[1], CultureInfo.InvariantCulture) - 1),
                                Normals = normals.ElementAt(int.Parse(faceIndices[2], CultureInfo.InvariantCulture) - 1)
                            };
                        });

                    var face = new Face()
                    {
                        MaterialName = currentMaterialName,
                        Indices = strides.Select((s, i) => (currentObject.Strides.Count()) + i).ToList()
                    };

                    currentObject.Strides.AddRange(strides);
                    currentObject.Faces.Add(face);
                    break;
                case "MTLLIB":
                    break;
                case "USEMTL":
                    currentMaterialName = words[1];
                    break;
                case "G":
                    currentObject.Name = words[1];
                    break;
                case "S":
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
