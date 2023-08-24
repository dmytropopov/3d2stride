using StrideGenerator.Services;
using System.Runtime.CompilerServices;

namespace StrideGenerator.Data;

public sealed class StrideOriginalIndexComparer : IComparer<Stride>
{
    public int Compare(Stride? x, Stride? y) => x.OriginalIndex.CompareTo(y.OriginalIndex);
}

public sealed class Stride : IComparable<Stride>
{
    public byte[] Data { get; }

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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void WriteInFormat(float data, ref byte* bytePtr, AttributeFormat attributeFormat)
    {
        switch (attributeFormat)
        {
            case AttributeFormat.Float:
                WriteFloat(data, ref bytePtr);
                break;
            case AttributeFormat.HalfFloat:
                WriteHalfFloat(data, ref bytePtr);
                break;
            case AttributeFormat.UnsignedByte:
                WriteSignedByte(data, ref bytePtr);
                break;
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteFloat(float data, ref byte* bytePtr)
    {
        *(float*)bytePtr = data;
        bytePtr += sizeof(float);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteHalfFloat(float data, ref byte* bytePtr)
    {
        *(Half*)bytePtr = (Half)data;
        bytePtr += sizeof(Half);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteSignedByte(float data, ref byte* bytePtr)
    {
        *bytePtr = (byte)(data * 255);
        bytePtr += sizeof(byte);
    }
}
