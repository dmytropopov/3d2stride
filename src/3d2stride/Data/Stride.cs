namespace StrideGenerator.Data;

public class Stride
{
    public double[] Coordinates { get; set; } = new double[3];
    public double[] Uvs { get; set; } = new double[2];
    public double[] Normals { get; set; } = new double[3];

    /// <summary>
    /// Vertex colors etc
    /// </summary>
    public Dictionary<string, double[]> ExtraAttributes;
}
