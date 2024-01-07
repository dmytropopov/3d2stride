using StrideGenerator.Data;

namespace StrideGenerator.Services;

public interface IInputReader
{
    public Task ReadInput(List<MeshObject> meshes, InputSettings inputData, StridePiece[] stridePieces, ProcessingPiece[] processingPieces, bool mergeObjects, int strideSize);
}
