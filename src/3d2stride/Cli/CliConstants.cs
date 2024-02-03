using StrideGenerator.Data;

namespace StrideGenerator.Services;

public static class CliConstants
{
    public static readonly Dictionary<string, (AttributeComponentType AttributeComponentType, AttributeType AttributeType, int DataIndex, string[]? TransformsTo, string HelpText)> AttributeInfos = new()
    {
        { "VT", new(AttributeComponentType.Unknown, AttributeType.Vertex, -1, [ "VTX", "VTY", "VTZ" ], "Shorthand for three vertex positions X,Y,Z. Does not work with bit-packed formats, use separate VTX, VTY, VTZ instead.") },
        { "VTX", new(AttributeComponentType.VertexX, AttributeType.Vertex, 0, null, "Vertex position X.") },
        { "VTY", new(AttributeComponentType.VertexY, AttributeType.Vertex, 1, null, "Vertex position Y.") },
        { "VTZ", new(AttributeComponentType.VertexZ, AttributeType.Vertex, 2, null, "Vertex position Z.") },
        { "N", new(AttributeComponentType.Unknown, AttributeType.Normal, -1, [ "NX", "NY", "NZ" ], "Shorthand for three normal values X,Y,Z. Does not work with bit-packed formats, use separate NX, NY, NZ instead.") },
        { "NX", new(AttributeComponentType.NormalX, AttributeType.Normal, 0, null, "Normal X value") },
        { "NY", new(AttributeComponentType.NormalY, AttributeType.Normal, 1, null, "Normal Y value") },
        { "NZ", new(AttributeComponentType.NormalZ, AttributeType.Normal, 2, null, "Normal Z value") },
        { "UV", new(AttributeComponentType.Unknown, AttributeType.TextureCoordinate, -1, [ "U", "V" ], "Shorthand for two texture coordinates U, V. Does not work with bit-packed formats, use separate U and V instead.") },
        { "U", new(AttributeComponentType.TextureCoordU, AttributeType.TextureCoordinate, 0, null, "Texture coordinate U") },
        { "V", new(AttributeComponentType.TextureCoordV, AttributeType.TextureCoordinate, 1, null, "Texture coordinate V") },
        { "E", new(AttributeComponentType.Empty, AttributeType.Empty, -1, null, "Leave empty") }
    };

    public static readonly Dictionary<string, (AttributeFormat AttributeFormat, string HelpText, int RequiredNumberOfComponents)> FormatInfos = new()
    {
        { "F", new(AttributeFormat.Float, $"Float", 0) },
        { "HF", new(AttributeFormat.HalfFloat, "Half-Float", 0) },
        { "NUB", new(AttributeFormat.NormalizedUnsignedByte, "Normalized Unsigned Byte", 0) },
        { "NSB", new(AttributeFormat.NormalizedSignedByte, "Normalized Signed Byte", 0) },
        { "UB", new(AttributeFormat.UnsignedByte, "Unsigned Byte", 0) },
        { "SB", new(AttributeFormat.SignedByte, "Signed Byte", 0) },
        { "SI2101010", new(AttributeFormat.SI2101010, "GL_INT_2_10_10_10. Not implemented yet.", 4 ) },
        { "SI2101010R", new(AttributeFormat.SI2101010R, "GL_INT_2_10_10_10_REV. Not implemented yet.", 4 ) },
        { "UI2101010", new(AttributeFormat.UI2101010, "GL_UNSIGNED_INT_2_10_10_10. Not implemented yet.", 4 ) },
        { "UI2101010R", new(AttributeFormat.UI2101010R, "GL_UNSIGNED_INT_2_10_10_10_REV. Not implemented yet.", 4 ) }
    };

    public static readonly Dictionary<string, (ProcessingType ProcesingType, string HelpText)> ProcessingInfos = new()
    {
        { "NORMV3", new(ProcessingType.NormalizeVec3, $"Normalize vec3") },
        { "NORM", new(ProcessingType.NormalizeFloat, $"Normalize float") },
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
