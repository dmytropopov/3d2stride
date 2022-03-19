namespace StrideGenerator.Data;

public class MeshObject
{
    public string? Name { get; set; }

    public List<Stride> Strides { get; set; } = new List<Stride>();
    public List<Face> Faces { get; set; } = new List<Face>();
}
