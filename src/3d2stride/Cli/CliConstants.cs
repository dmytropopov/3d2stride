using StrideGenerator.Data;

namespace StrideGenerator.Services;

public static class CliConstants
{
    public static readonly Dictionary<string, (AttributeType AttributeType, string[]? TransformsTo, string HelpText)> AttributeInfos = new()
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

    public static readonly Dictionary<string, (AttributeFormat AttributeFormat, string HelpText)> FormatInfos = new()
    {
        { "F", new(AttributeFormat.Float, $"Float") },
        { "HF", new(AttributeFormat.HalfFloat, "Half-Float") },
        { "NUB", new(AttributeFormat.NormalizedUnsignedByte, "Normalized Unsigned Byte") },
        { "NSB", new(AttributeFormat.NormalizedSignedByte, "Normalized Signed Byte") },
        { "UB", new(AttributeFormat.UnsignedByte, "Unsigned Byte") },
        { "SB", new(AttributeFormat.SignedByte, "Signed Byte") },
        { "SI2101010", new(AttributeFormat.SI2101010, "GL_INT_2_10_10_10. Not implemented yet." ) },
        { "SI2101010R", new(AttributeFormat.SI2101010R, "GL_INT_2_10_10_10_REV. Not implemented yet." ) }
    };

    public static readonly Dictionary<string, (ProcessingType ProcesingType, string HelpText)> ProcessingInfos = new()
    {
        { "NORMV3", new(ProcessingType.NormalizeVec3, $"Normalize vec3") },
        //{ "NORM", new(ProcessingType.NormalizeFloat, $"Normalize float") },
        { "NEG", new(ProcessingType.Negate, "Negate") },
        { "ONEMIN", new(ProcessingType.OneMinus, "One minus (1 - <attibute-value>)") }
    };

    public static readonly Dictionary<string, (IndexFormat IndexFormat, string HelpText)> IndexFormatInfos = new()
    {
        { "A", new(IndexFormat.Byte, $"Auto (smallest)") },
        { "B", new(IndexFormat.Byte, $"Byte") },
        { "S", new(IndexFormat.Short, $"Short") },
        { "I", new(IndexFormat.Integer, "Integer") }
    };
}
