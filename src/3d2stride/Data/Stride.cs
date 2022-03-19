namespace StrideGenerator.Data;

public class Stride : IEquatable<Stride>
{
    public double[] Coordinates { get; set; } = new double[3];
    public double[] Uvs { get; set; } = new double[2];
    public double[] Normals { get; set; } = new double[3];

    /// <summary>
    /// Vertex colors etc
    /// </summary>
    public Dictionary<string, double[]> ExtraAttributes;

    public bool Equals(Stride? other)
    {
        if (other == null)
        {
            return false;
        }

        return Coordinates.SequenceEqual(other.Coordinates) 
            && Uvs.SequenceEqual(other.Uvs)
            && Normals.SequenceEqual(other.Normals);
        // TODO ExtraAttributes
    }

    public override bool Equals(object obj) => Equals(obj as Stride);

    public override int GetHashCode() => 0;// HashCode.Combine(Coordinates.GetHashCode(), Uvs.GetHashCode(), Normals.GetHashCode());
}
