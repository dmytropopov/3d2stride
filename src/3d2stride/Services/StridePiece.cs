using StrideGenerator.Data;

namespace StrideGenerator.Services;

public record FormatComponent(int InputIndex, int DataPosition, AttributeComponentType AttributeComponentType);

public readonly record struct StridePiece(AttributeFormat Format, int TotalSize, int Offset, List<FormatComponent> AttributeTypesPerInput);

public record struct OutputSettings(string FileName, List<StridePiece> StrideMap, List<ProcessingPiece> ProcessingMap, bool MergeObjects, int Alignment)
{
    public readonly int GetStrideSize() => StrideMap.Sum(x => x.TotalSize);
}
