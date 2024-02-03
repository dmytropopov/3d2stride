using StrideGenerator.Services;
using System.Runtime.CompilerServices;

namespace StrideGenerator.Data;

public sealed partial class Stride
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void WriteInFormat(float data, byte* bytePtr, AttributeFormat attributeFormat, int dataPosition)
    {
        switch (attributeFormat)
        {
            case AttributeFormat.Float:
                WriteFloat(data, bytePtr);
                break;
            case AttributeFormat.HalfFloat:
                WriteHalfFloat(data, bytePtr);
                break;
            case AttributeFormat.NormalizedUnsignedByte:
                WriteNormalizedUnsignedByte(data, bytePtr);
                break;
            case AttributeFormat.NormalizedSignedByte:
                WriteNormalizedSignedByte(data, bytePtr);
                break;
            case AttributeFormat.UnsignedByte:
                WriteUnsignedByte(data, bytePtr);
                break;
            case AttributeFormat.SignedByte:
                WriteUnsignedByte(data, bytePtr);
                break;
            case AttributeFormat.SI2101010:
                WriteSI2101010(data, bytePtr, dataPosition);
                break;
            case AttributeFormat.SI2101010R:
                break;
            case AttributeFormat.UI2101010:
                break;
            case AttributeFormat.UI2101010R:
                break;
        };
    }

    private unsafe void WriteSI2101010(float data, byte* bytePtr, int dataPosition)
    {
        switch (dataPosition)
        {
            case 0: // 2 bits
                break;
            case 1: // 10 bits
                break;
            case 2: // 10 bits
                break;
            case 3: // 10 bits
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteFloat(float data, byte* bytePtr)
    {
        *(float*)bytePtr = data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteHalfFloat(float data, byte* bytePtr)
    {
        *(Half*)bytePtr = (Half)data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteUnsignedByte(float data, byte* bytePtr)
    {
        // TODO: check range
        *bytePtr = (byte)(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteSignedByte(float data, byte* bytePtr)
    {
        // TODO: check range
        *(sbyte*)bytePtr = (sbyte)data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteNormalizedUnsignedByte(float data, byte* bytePtr)
    {
        // TODO: check range
        *bytePtr = (byte)(data * 255);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void WriteNormalizedSignedByte(float data, byte* bytePtr)
    {
        // TODO: check range
        *(sbyte*)bytePtr = (sbyte)(data * 127);
    }
}
