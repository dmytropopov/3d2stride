namespace StrideGenerator.Data;

public sealed class MeshObject
{
    public string? Name { get; set; }

    public ICollection<Stride> Strides { get; set; } = new List<Stride>(4 * 65536);
    public List<Face> Faces { get; set; } = new List<Face>(65536);
}
