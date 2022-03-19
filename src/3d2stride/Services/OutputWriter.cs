using StrideGenerator.Data;

namespace StrideGenerator.Services;

public class OutputWriter : IOutputWriter
{
    public async Task Write(IEnumerable<MeshObject> meshes, OutputSettings outputSettings)
    {
        WriteIndices(meshes, outputSettings);
        WriteStrides(meshes, outputSettings);
    }

    private void WriteStrides(IEnumerable<MeshObject> meshes, OutputSettings outputSettings)
    {
        using var stream = File.Open(Path.ChangeExtension(outputSettings.FileName + "-strides", "bin"), FileMode.Create);
        using var writer = new BinaryWriter(stream);
        foreach (var mesh in meshes)
        {
            foreach (var meshObject in meshes)
            {
                foreach (var face in meshObject.Faces)
                {
                    foreach (var index in face.Indices)
                    {
                        var stride = meshObject.Strides.ElementAt(index);
                        writer.Write((float)stride.Coordinates[0]);
                        writer.Write((float)stride.Coordinates[1]);
                        writer.Write((float)stride.Coordinates[2]);
                        writer.Write((float)stride.Uvs[0]);
                        writer.Write((float)stride.Uvs[1]);
                    }
                }
            }
        }
        writer.Close();
        stream.Close();
    }

    private static void WriteIndices(IEnumerable<MeshObject> meshes, OutputSettings outputSettings)
    {
        using var stream = File.Open(Path.ChangeExtension(outputSettings.FileName + "-indices", "bin"), FileMode.Create);
        using var writer = new BinaryWriter(stream);
        foreach (var mesh in meshes)
        {
            foreach (var meshObject in meshes)
            {
                foreach (var face in meshObject.Faces)
                {
                    foreach (var index in face.Indices)
                    {
                        writer.Write((ushort)index);
                    }
                }
            }
        }
        writer.Close();
        stream.Close();
    }
}
