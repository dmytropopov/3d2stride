namespace StrideGenerator.Data;

public sealed class StrideOriginalIndexComparer : IComparer<Stride>
{
    public int Compare(Stride? x, Stride? y) => x.OriginalIndex.CompareTo(y.OriginalIndex);
}

public sealed partial class Stride(int size) : IComparable<Stride>
{
    public byte[] Data { get; } = new byte[size];

    public Face Face { get; set; }
    public int OriginalIndexInFace { get; set; }
    public int Index { get; set; } = -1;
    public int OriginalIndex { get; set; } = -1;

    public override int GetHashCode() => 0;

    public int CompareTo(Stride other) => Data.AsSpan().SequenceCompareTo(other.Data);
}
