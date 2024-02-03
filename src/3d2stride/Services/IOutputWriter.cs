using StrideGenerator.Data;

namespace StrideGenerator.Services;

public interface IOutputWriter
{
    Task Write(List<MeshObject> meshes, IEnumerable<InputSettings> inputs, OutputSettings outputSettings);
}
