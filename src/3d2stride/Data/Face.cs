namespace StrideGenerator.Data;

public sealed class Face
{
    public List<Stride> Strides { get; set; } = new();
    public string? MaterialName { get; set; }
}
