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
    VertexX,
    VertexY,
    VertexZ,
    NormalX,
    NormalY,
    NormalZ,
    TextureCoordU,
    TextureCoordV,
    Empty
}

public readonly record struct StridePiece(AttributeFormat Format, int TotalSize, int InputIndex, int Offset, AttributeType[] AttributeTypes);

public readonly record struct AttributeInfo(AttributeType AttributeType, string[]? TransformsTo, string HelpText);

public readonly record struct InputAttributes(string[] Attributes);

public partial class OutputAttributes
{
    public static readonly Dictionary<string, AttributeInfo> AttributesInfos = new()
    {
        { "VT", new(AttributeType.Unknown, [ "VTX", "VTY", "VTZ" ], "Shorthand for three vertex positions X,Y,Z. Does not work with bit-packed formats, use separate VTX, VTY, VTZ instead.") },
        { "VTX", new(AttributeType.VertexX, null, "Vertex position X.") },
        { "VTY", new(AttributeType.VertexY, null, "Vertex position Y.") },
        { "VTZ", new(AttributeType.VertexZ, null, "Vertex position Z.") },
        { "N", new(AttributeType.Unknown, [ "NX", "NY", "NZ" ], "Shorthand for three normal values X,Y,Z. Does not work with bit-packed formats, use separate NX, NY, NZ instead.") },
        { "NX", new(AttributeType.NormalX, null, "Normal X value") },
        { "NY", new(AttributeType.NormalY, null, "Normal Y value") },
        { "NZ", new(AttributeType.NormalZ, null, "Normal Z value") },
        { "UV", new(AttributeType.Unknown, [ "U", "V" ], "Shorthand for two texture coordinates U, V. Does not work with bit-packed formats, use separate U and V instead.") },
        { "U", new(AttributeType.TextureCoordU, null, "Texture coordinate U") },
        { "V", new(AttributeType.TextureCoordV, null, "Texture coordinate V") },
        { "E", new(AttributeType.Empty, null, "Leave empty") }
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

    public static readonly Dictionary<string, (AttributeFormat AttributeFormat, string HelpText)> AttributeFormats = new()
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

    public List<StridePiece> StrideMap { get; private set; } = [];

    [GeneratedRegex("([a-zA-Z]*)(\\d*)")]
    private static partial Regex AttributeRegex();

    public static OutputAttributes Parse(string input)
    {
        input = input.ToUpperInvariant();
        var result = new OutputAttributes();
        int offset = 0;

        foreach (var part in input.Split(','))
        {
            var subparts = part.Split(':');
            var attribute = subparts[0].ToUpperInvariant();
            var format = subparts[1].ToUpperInvariant();

            var matchGroups = AttributeRegex().Matches(attribute)[0].Groups;
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

            if (attributeInfo.TransformsTo == null)
            {
                AddAttribute(result, attributeInfo, index, attributeFormat, ref offset);
            }
            else
            {
                foreach (var tranform in attributeInfo.TransformsTo)
                {
                    AddAttribute(result, AttributesInfos[tranform], index, attributeFormat, ref offset);
                }
            }
        }

        return result;
    }

    private static void AddAttribute(OutputAttributes result, AttributeInfo attributeInfo, int inputIndex, (AttributeFormat AttributeFormat, string HelpText) attributeFormat, ref int offset)
    {
        var sizeInBytes = FormatSizes[attributeFormat.AttributeFormat];
        result.StrideMap.Add(new(attributeFormat.AttributeFormat, sizeInBytes, inputIndex, offset, [attributeInfo.AttributeType /*bit-packed attribute formats not parsed yet*/ ]));
        offset += sizeInBytes;
    }

    public int GetStrideSize() => StrideMap.Sum(x => x.TotalSize);
}

public readonly record struct InputSettings(string FileName, string FileFormat, InputAttributes InputAttributes, Verbosity Verbosity);

public record struct OutputSettings(string FileName, OutputAttributes OutputAttributes, bool MergeObjects, int Alignment);
