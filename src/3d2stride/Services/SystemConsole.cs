namespace StrideGenerator.Services;

public class SystemConsole : IConsole
{
    private readonly GlobalOptions _globalOptions;

    public SystemConsole(GlobalOptions globalOptions)
    {
        _globalOptions = globalOptions;
    }

    public void WriteLine(string line)
    {
        if (_globalOptions.Verbosity > Verbosity.Silent)
        {
            Console.WriteLine(line);
        }
    }
}
