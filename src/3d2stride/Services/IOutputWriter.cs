using StrideGenerator.Data;

namespace StrideGenerator.Services;

public interface IOutputWriter
{
    Task Write(IEnumerable<MeshObject> meshes, IEnumerable<InputSettings> inputs, OutputSettings outputSettings);
}
