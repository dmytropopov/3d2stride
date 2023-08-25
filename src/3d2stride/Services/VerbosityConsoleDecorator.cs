using McMaster.Extensions.CommandLineUtils;

namespace StrideGenerator.Services;

public class VerbosityConsoleDecorator : IConsole
{
    private readonly GlobalOptions _globalOptions;
    private readonly IConsole _console;
    private readonly StringWriter _nullWriter;

    public VerbosityConsoleDecorator(IConsole console, GlobalOptions globalOptions)
    {
        _globalOptions = globalOptions;
        _console = console;
        _nullWriter = new StringWriter();
    }

    public TextWriter Out => _globalOptions.Verbosity > Verbosity.Silent ? _console.Out : _nullWriter;

    public TextWriter Error => _console.Error;

    public TextReader In => _console.In;

    public bool IsInputRedirected => _console.IsInputRedirected;

    public bool IsOutputRedirected => _console.IsOutputRedirected;

    public bool IsErrorRedirected => _console.IsErrorRedirected;

    public ConsoleColor ForegroundColor { get => _console.ForegroundColor; set => _console.ForegroundColor = value; }
    public ConsoleColor BackgroundColor { get => _console.BackgroundColor; set => _console.BackgroundColor = value; }

    public event ConsoleCancelEventHandler? CancelKeyPress;

    public void ResetColor() => _console.ResetColor();
}
