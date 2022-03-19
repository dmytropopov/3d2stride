using StrideGenerator.Data;

namespace StrideGenerator.Services;

public interface IInputReader
{
    public Task<IEnumerable<MeshObject>> ReadInput(InputData inputData);
}
