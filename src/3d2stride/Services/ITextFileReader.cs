namespace StrideGenerator.Services;

public interface ITextFileReader
{
    IEnumerable<string> ReadLines(InputSettings inputData);
}
