namespace StrideGenerator.Services;

public class TextFileReader : ITextFileReader
{
    public IEnumerable<string> ReadLines(InputSettings inputData) => File.ReadLines(inputData.FileName);
}
