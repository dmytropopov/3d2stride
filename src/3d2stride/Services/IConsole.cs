namespace StrideGenerator.Services;

public class GlobalOptions
{
    public Verbosity Verbosity { get; set; }
}

public enum Verbosity
{
    Silent = 0,
    Normal = 1
}

public interface IConsole
{
    void WriteLine(string line);
}
