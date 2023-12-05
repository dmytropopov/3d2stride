namespace StrideGenerator.Services;

public class SystemConsole(GlobalOptions globalOptions) : IConsole
{
    private readonly GlobalOptions _globalOptions = globalOptions;

    public void WriteLine(string line)
    {
        if (_globalOptions.Verbosity > Verbosity.Silent)
        {
            Console.WriteLine(line);
        }
    }
}
