namespace StrideGenerator.Data;

public sealed class StrideOriginalIndexComparer : IComparer<Stride>
{
    public int Compare(Stride? x, Stride? y) => x.OriginalIndex.CompareTo(y.OriginalIndex);
}

public sealed class Stride : IComparable<Stride>
{
    public byte[] Data { get; }
    //public double[] Coordinates { get; set; }
    //public double[] Uvs { get; set; }
    //public double[] Normals { get; set; }

    public Face Face { get; set; }
    public int OriginalIndexInFace { get; set; }
    public int Index { get; set; } = -1;
    public int OriginalIndex { get; set; } = -1;

    /// <summary>
    /// Vertex colors etc
    /// </summary>
    public Dictionary<string, double[]> ExtraAttributes;

    public override int GetHashCode() => 0;

    public Stride(int size)
    {
        Data = new byte[size];
    }

    public int CompareTo(Stride other)
    {
        return Data.AsSpan().SequenceCompareTo(other.Data);
        //for (var i = 0; i < Data.Length; i++)
        //{
        //    var comparison = Data[i] - other.Data[i];
        //    if (comparison != 0)
        //    {
        //        return comparison;
        //    }
        //}
        //return 0;
        //var compare0 = Coordinates[0].CompareTo(other.Coordinates[0]);
        //if (compare0 != 0) return compare0;

        //var compare1 = Coordinates[1].CompareTo(other.Coordinates[1]);
        //if (compare1 != 0) return compare1;

        //var compare2 = Coordinates[2].CompareTo(other.Coordinates[2]);
        //if (compare2 != 0) return compare2;

        //var compare3 = Uvs[0].CompareTo(other.Uvs[0]);
        //if (compare3 != 0) return compare3;

        //var compare4 = Uvs[1].CompareTo(other.Uvs[1]);
        //return compare4;
    }
}
