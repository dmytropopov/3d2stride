namespace StrideGenerator.Services;

public interface IGenerator
{
    Task Generate(IEnumerable<InputSettings> inputs, OutputSettings output);
}