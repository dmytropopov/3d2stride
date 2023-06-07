using System.Text.RegularExpressions;

namespace StrideGenerator.Services;

public enum AttributeFormat
{
    Unknown = 0,
    Float,
    HalfFloat
}

public enum AttributeType
{
    Unknown = 0,
    Vertex,
    Normal,
    TextureCoords,
    TextureCoordU,
    TextureCoordV
}

public readonly record struct Attribute(AttributeInfo AttributeInfo, int Index, AttributeFormat Format, int totalSize);

public readonly record struct AttributeInfo(AttributeType AttributeType, int Size);

public readonly record struct InputAttributes(string[] Attributes);

public class OutputAttributes
{
    private static readonly Dictionary<string, AttributeInfo> attributesInfo = new()
    {
        { "VT", new(AttributeType.Vertex, 3) },
        { "N", new(AttributeType.Normal, 3) },
        { "UV", new(AttributeType.TextureCoords, 2) },
        { "U", new(AttributeType.TextureCoordU, 1) },
        { "V", new(AttributeType.TextureCoordV, 1) }
    };

    private static readonly Dictionary<AttributeFormat, int> formatSizes = new()
    {
        { AttributeFormat.Float, sizeof(float) },
        { AttributeFormat.HalfFloat, sizeof(float) / 2 }
    };

    public List<Attribute> Attributes { get; private set; } = new();

    public static OutputAttributes Parse(string input)
    {
        input = input.ToUpperInvariant();
        var result = new OutputAttributes();

        foreach (var part in input.Split(','))
        {
            var subparts = part.Split(':');
            var attribute = subparts[0];
            var format = subparts[1];

            var matchGroups = Regex.Matches(attribute, "([a-zA-Z]*)(\\d*)")[0].Groups;
            AttributeInfo attributeInfo;
            if (matchGroups.Count == 3)
            {
                if (!attributesInfo.TryGetValue(matchGroups[1].Value, out attributeInfo))
                {
                    throw new Exception("Output attribute not recognized: " + matchGroups[1].Value);
                }
            }
            else
            {
                throw new Exception("Invalid format of attribute: " + attribute);
            }

            var index = int.Parse(matchGroups[2].Value);

            AttributeFormat attributeFormat = format switch
            {
                "F" => AttributeFormat.Float,
                "HF" => AttributeFormat.HalfFloat,
                _ => throw new NotImplementedException("Output format not recognized: " + format),
            };

            var sizeInBytes = formatSizes[attributeFormat] * attributeInfo.Size;
            result.Attributes.Add(new(attributeInfo, index, attributeFormat, sizeInBytes));
        }

        return result;
    }

    public int GetStrideSize() => Attributes.Sum(x => x.totalSize);
}

public readonly record struct InputSettings(string FileName, string FileFormat, InputAttributes InputAttributes);

public record struct OutputSettings(string FileName, OutputAttributes OutputAttributes);