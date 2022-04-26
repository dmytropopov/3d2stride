namespace StrideGenerator.Services;

public enum AttributeFormat
{
    Unknown = 0,
    Float,
    HalfFloat
}

public static class Attributes
{
    public const string Vertex = "V";
    public const string Normal = "N";
    public const string TextureCoord = "UV";
}

public readonly record struct Attribute(string name, AttributeFormat format);

public readonly record struct InputAttributes(string[] Attributes);

public readonly record struct OutputAttributes(string[] Attributes)
{
    // TODO
    public int GetStrideSize() => sizeof(float) * 5;
}

public readonly record struct InputSettings(string FileName, string FileFormat, InputAttributes InputAttributes);

public record struct OutputSettings(string FileName, OutputAttributes OutputAttributes);