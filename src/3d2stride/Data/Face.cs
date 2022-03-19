namespace StrideGenerator.Data;

public class Face
{
    public List<FaceVertex> FaceVertices { get; set; } = new List<FaceVertex>();
    public string? MaterialName { get; set; }
}
