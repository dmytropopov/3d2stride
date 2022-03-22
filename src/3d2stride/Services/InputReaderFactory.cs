using StrideGenerator.Services.Obj;

namespace StrideGenerator.Services;

public sealed class InputReaderFactory
{
    private readonly Dictionary<string, IInputReader> _inputReaders;

    public InputReaderFactory(IEnumerable<IInputReader> inputReaders)
    {
        _inputReaders = new()
        {
            { Constants.FileFormats.Obj, inputReaders.OfType<ObjReader>().Single() }
        };
    }

    public IInputReader GetReader(string fileType) => _inputReaders[fileType];
}
