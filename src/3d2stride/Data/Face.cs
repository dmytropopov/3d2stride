namespace StrideGenerator.Data;

public class Face
{
    public List<Stride> Strides { get; set; } = new();
    public string? MaterialName { get; set; }
}
