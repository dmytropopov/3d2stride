using StrideGenerator.Data;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace StrideGenerator.Services;

/// <summary>
/// Processor is always the same, and is in hot-path.
/// Static class is faster than DI
/// </summary>
public static class StrideDataProcessor
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Process(float[] data, ProcessingPiece[] processingPieces)
    {
        for (int i = 0; i < processingPieces.Length; i++)
        {
            var piece = processingPieces[i];

            switch (piece.ProcessingType)
            {
                case ProcessingType.NormalizeFloat:
                    // Needs to know min/max value, bit-packed formats can be tricky
                    break;
                case ProcessingType.NormalizeVec3:
                    var result = Vector3.Normalize(new(data[0], data[1], data[2]));
                    data[0] = result[0];
                    data[1] = result[1];
                    data[2] = result[2];
                    break;
                case ProcessingType.OneMinus:
                    data[piece.DataIndex] = 1.0f - data[piece.DataIndex];
                    break;
                case ProcessingType.Negate:
                    data[piece.DataIndex] = -data[piece.DataIndex];
                    break;
            }
        }
    }
}
