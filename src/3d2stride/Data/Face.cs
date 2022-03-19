namespace StrideGenerator.Data;

public class Face
{
    public List<ulong> Indices { get; set; } = new List<ulong>();
    public string? MaterialName { get; set; }
}
