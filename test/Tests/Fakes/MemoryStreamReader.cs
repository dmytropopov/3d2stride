using StrideGenerator.Services;
using System.Text;

namespace Tests.Fakes;

public class MemoryStreamReader : ITextFileReader
{
    //private readonly MemoryStream _memoryStream;
    private readonly List<string> _lines;
    
    public MemoryStreamReader(string fileName)
    {
        //_memoryStream = new MemoryStream();

        //using var fileStram = File.OpenRead(fileName);
        //fileStram.CopyTo(_memoryStream);
        _lines = File.ReadLines(fileName).ToList();
    }

    public IEnumerable<string> ReadLines(InputSettings inputData)
    {
        return _lines;
        //_memoryStream.Position = 0;
        //return ReadLines(_memoryStream, Encoding.ASCII);
    }

    private IEnumerable<string> ReadLines(Stream stream, Encoding encoding)
    {
        using (var reader = new StreamReader(stream, encoding))
        {
            string line;
            while ((line = reader.ReadLine()!) != null)
            {
                yield return line;
            }
        }
    }
}
