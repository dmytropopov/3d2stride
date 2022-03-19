using StrideGenerator.Data;

namespace StrideGenerator.Services;

public class OutputWriter : IOutputWriter
{
    public async Task Write(IEnumerable<MeshObject> meshes, OutputSettings outputSettings)
    {
        using var indicesStream = File.Open(Path.ChangeExtension(outputSettings.FileName + "-indices2", "bin"), FileMode.Create);
        using var indicesWriter = new BinaryWriter(indicesStream);

        foreach (var mesh in meshes)
        {
            foreach (var meshObject in meshes)
            {
                foreach (var face in meshObject.Faces)
                {
                    foreach (var index in face.Indices)
                    {
                        indicesWriter.Write((ushort)index);
                        //indicesWriter.Close();
                        //indicesStream.Close();
                        //return;
                    }
                    // TODO
                    //indicesWriter.Flush();
                }
            }
        }
        indicesWriter.Close();
        indicesStream.Close();
    }
}
