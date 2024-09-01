using StrideGenerator.Data;

namespace StrideGenerator.Services;

public static class Constants
{
    public static class FileFormats
    {
        public const string Obj = "OBJ";
    }

    public static readonly Dictionary<AttributeFormat, int> FormatSizes = new()
    {
        { AttributeFormat.Float, sizeof(float) },
        { AttributeFormat.HalfFloat, sizeof(float) / 2 },
        { AttributeFormat.NormalizedUnsignedByte, sizeof(byte) },
        { AttributeFormat.NormalizedSignedByte, sizeof(byte) },
        { AttributeFormat.UnsignedByte, sizeof(byte) },
        { AttributeFormat.SignedByte, sizeof(byte) },
        { AttributeFormat.SI2101010, 4 },
        { AttributeFormat.SI2101010R, 4 },
        { AttributeFormat.UI2101010, 4 },
        { AttributeFormat.UI2101010R, 4 }
    };

    public static readonly Dictionary<AttributeComponentType, AttributeType> AttributeTypes = new()
    {
        { AttributeComponentType.VertexX, AttributeType.Vertex },
        { AttributeComponentType.VertexY, AttributeType.Vertex },
        { AttributeComponentType.VertexZ, AttributeType.Vertex },
        { AttributeComponentType.NormalX, AttributeType.Normal },
        { AttributeComponentType.NormalY, AttributeType.Normal },
        { AttributeComponentType.NormalZ, AttributeType.Normal },
        { AttributeComponentType.TextureCoordU, AttributeType.TextureCoordinate },
        { AttributeComponentType.TextureCoordV, AttributeType.TextureCoordinate }
    };
}
