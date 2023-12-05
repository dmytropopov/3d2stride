using StrideGenerator.Data;

namespace StrideGenerator.Services;

public interface IInputReader
{
    public Task ReadInput(List<MeshObject> meshes, InputSettings inputData, List<StridePiece> stridePieces, bool mergeObjects, int strideSize);
}
