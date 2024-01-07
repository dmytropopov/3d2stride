using StrideGenerator.Data;

namespace StrideGenerator.Services;

public readonly record struct ProcessingPiece(int InputIndex, ProcessingType ProcessingType, AttributeType AttributeType, int DataIndex);
