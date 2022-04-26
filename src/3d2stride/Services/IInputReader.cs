using StrideGenerator.Data;

namespace StrideGenerator.Services;

public interface IInputReader
{
    public Task<IEnumerable<MeshObject>> ReadInput(InputSettings inputData, OutputSettings outputSettings);
}
