using System.Text.RegularExpressions;

namespace StrideGenerator.Services;

public class GlobalOptions
{
    public Verbosity Verbosity { get; set; }
}

public enum Verbosity
{
    Silent = 0,
    Normal = 1
}

public enum AttributeFormat
{
    Unknown = 0,
    Float,
    HalfFloat,
    NormalizedUnsignedByte,
    NormalizedSignedByte,
    UnsignedByte,
    SignedByte,
    SI2101010,
    SI2101010R,
}

public enum AttributeType
{
    Unknown = 0,
    Vertex,
    Normal,
    TextureCoords,
    TextureCoordU,
    TextureCoordV,
    Empty
}

public readonly record struct Attribute(AttributeInfo AttributeInfo, int Index, AttributeFormat Format, int totalSize);

public readonly record struct AttributeInfo(AttributeType AttributeType, int Size, string HelpText);

public readonly record struct InputAttributes(string[] Attributes);

public class OutputAttributes
{
    public static readonly Dictionary<string, AttributeInfo> AttributesInfos = new()
    {
        { "VT", new(AttributeType.Vertex, 3, "Shorthand for three vertex positions X,Y,Z. Does not work with bit-packed formats, use separate VTX, VTY, VTZ instead.") },
        { "VTX", new(AttributeType.Vertex, 1, "Vertex position X.") },
        { "VTY", new(AttributeType.Vertex, 1, "Vertex position Y.") },
        { "VTZ", new(AttributeType.Vertex, 1, "Vertex position Z.") },
        { "N", new(AttributeType.Normal, 3, "Shorthand for three normal values X,Y,Z. Does not work with bit-packed formats, use separate NX, NY, NZ instead.") },
        { "NX", new(AttributeType.Normal, 1, "Normal X value") },
        { "NY", new(AttributeType.Normal, 1, "Normal Y value") },
        { "NZ", new(AttributeType.Normal, 1, "Normal Z value") },
        { "UV", new(AttributeType.TextureCoords, 2, "Shorthand for two texture coordinates U, V. Does not work with bit-packed formats, use separate U and V instead.") },
        { "U", new(AttributeType.TextureCoordU, 1, "Texture coordinate U") },
        { "V", new(AttributeType.TextureCoordV, 1, "Texture coordinate V") },
        { "E", new(AttributeType.Empty, 0, "Leave empty") }
    };

    public static readonly Dictionary<AttributeFormat, int> FormatSizes = new()
    {
        { AttributeFormat.Float, sizeof(float) },
        { AttributeFormat.HalfFloat, sizeof(float) / 2 },
        { AttributeFormat.NormalizedUnsignedByte, sizeof(byte) },
        { AttributeFormat.NormalizedSignedByte, sizeof(byte) },
        { AttributeFormat.UnsignedByte, sizeof(byte) },
        { AttributeFormat.SignedByte, sizeof(byte) },
        { AttributeFormat.SI2101010, 4 },
        { AttributeFormat.SI2101010R, 4 }
    };

    public static readonly Dictionary<string, (AttributeFormat AttributeFormat, string HelpText)> AttributeFormats = new ()
    {
        { "F", new(AttributeFormat.Float, $"Float") },
        { "HF", new( AttributeFormat.HalfFloat, "Half-Float") },
        { "NUB", new( AttributeFormat.NormalizedUnsignedByte, "Normalized Unsigned Byte") },
        { "NSB", new( AttributeFormat.NormalizedSignedByte, "Normalized Signed Byte") },
        { "UB", new( AttributeFormat.UnsignedByte, "Unsigned Byte") },
        { "SB", new( AttributeFormat.SignedByte, "Signed Byte") },
        { "SI2101010", new( AttributeFormat.SI2101010, "GL_INT_2_10_10_10. Not implemented yet." ) },
        { "SI2101010R", new( AttributeFormat.SI2101010R, "GL_INT_2_10_10_10_REV. Not implemented yet." ) }
    };

    public List<Attribute> Attributes { get; private set; } = new();

    public static OutputAttributes Parse(string input)
    {
        input = input.ToUpperInvariant();
        var result = new OutputAttributes();

        foreach (var part in input.Split(','))
        {
            var subparts = part.Split(':');
            var attribute = subparts[0].ToUpperInvariant();
            var format = subparts[1].ToUpperInvariant();

            var matchGroups = Regex.Matches(attribute, "([a-zA-Z]*)(\\d*)")[0].Groups;
            AttributeInfo attributeInfo;
            if (matchGroups.Count == 3)
            {
                if (!AttributesInfos.TryGetValue(matchGroups[1].Value, out attributeInfo))
                {
                    throw new Exception("Output attribute not recognized: " + matchGroups[1].Value);
                }
            }
            else
            {
                throw new Exception("Invalid format of attribute: " + attribute);
            }

            var index = int.Parse(matchGroups[2].Value);

            if (!AttributeFormats.TryGetValue(format, out var attributeFormat))
            {
                throw new NotImplementedException("Output format not recognized: " + format);
            }

            var sizeInBytes = FormatSizes[attributeFormat.AttributeFormat] * attributeInfo.Size;
            result.Attributes.Add(new(attributeInfo, index, attributeFormat.AttributeFormat, sizeInBytes));
        }

        return result;
    }

    public int GetStrideSize() => Attributes.Sum(x => x.totalSize);
}

public readonly record struct InputSettings(string FileName, string FileFormat, InputAttributes InputAttributes, Verbosity Verbosity);

public record struct OutputSettings(string FileName, OutputAttributes OutputAttributes, bool MergeObjects, int Alignment);
