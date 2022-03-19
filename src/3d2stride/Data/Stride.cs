namespace StrideGenerator.Data;

public class Stride : /*IEquatable<Stride>,*/ IComparable<Stride>
{
    public double[] Coordinates { get; set; } = new double[3];
    public double[] Uvs { get; set; } = new double[2];
    public double[] Normals { get; set; } = new double[3];

    public Face Face { get; set; }
    public int OriginalIndexInFace { get; set; }

    /// <summary>
    /// Vertex colors etc
    /// </summary>
    public Dictionary<string, double[]> ExtraAttributes;

    //private int _hashCode;

    //private CalculateHash()
    //{
    //    _hashCode = HashCode.Combine(Coordinates[0].GetHashCode() ^ Coordinates[1].GetHashCode() ^ Coordinates[2].GetHashCode()
    //        ^ Uvs[0].GetHashCode() ^ Uvs[1].GetHashCode()
    //        /*^ Normals[0].GetHashCode() ^ Normals[1].GetHashCode() ^ Normals[2].GetHashCode()*/
    //        );
    //}

    //public bool Equals(Stride? other)
    //{
    //    if (other == null)
    //    {
    //        return false;
    //    }

    //    //return Coordinates.SequenceEqual(other.Coordinates)
    //    //    && Uvs.SequenceEqual(other.Uvs);
    //        //&& Normals.SequenceEqual(other.Normals);
    //    // TODO ExtraAttributes

    //    return Coordinates[0] == other.Coordinates[0] && Coordinates[1] == other.Coordinates[1] && Coordinates[2] == other.Coordinates[2]
    //        && Uvs[0] == other.Uvs[0] && Uvs[1] == other.Uvs[1];
    //}

    //public override bool Equals(object obj) => Equals(obj as Stride);

    //public override int GetHashCode()
    //{
    //    return HashCode.Combine(Coordinates[0].GetHashCode() ^ Coordinates[1].GetHashCode() ^ Coordinates[2].GetHashCode()
    //        ^ Uvs[0].GetHashCode() ^ Uvs[1].GetHashCode()
    //        /*^ Normals[0].GetHashCode() ^ Normals[1].GetHashCode() ^ Normals[2].GetHashCode()*/
    //        );
    //}

    public override int GetHashCode() => 0;// HashCode.Combine(Coordinates.GetHashCode(), Uvs.GetHashCode(), Normals.GetHashCode());

    public int CompareTo(Stride? other)
    {
        var compare0 = Coordinates[0].CompareTo(other.Coordinates[0]);
        if (compare0 != 0) return compare0;

        var compare1 = Coordinates[1].CompareTo(other.Coordinates[1]);
        if (compare1 != 0) return compare1;

        var compare2 = Coordinates[2].CompareTo(other.Coordinates[2]);
        if (compare2 != 0) return compare2;

        var compare3 = Uvs[0].CompareTo(other.Uvs[0]);
        if (compare3 != 0) return compare3;

        var compare4 = Uvs[1].CompareTo(other.Uvs[1]);
        return compare4;
    }
}
