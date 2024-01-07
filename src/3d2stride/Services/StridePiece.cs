using StrideGenerator.Data;

namespace StrideGenerator.Services;

public readonly record struct StridePiece(AttributeFormat Format, int TotalSize, int InputIndex, int Offset, AttributeComponentType[] AttributeTypes);

public record struct OutputSettings(string FileName, List<StridePiece> StrideMap, List<ProcessingPiece> ProcessingMap, bool MergeObjects, int Alignment)
{
    public readonly int GetStrideSize() => StrideMap.Sum(x => x.TotalSize);
}
