using BenchmarkDotNet.Attributes;
using StrideGenerator.Services.Obj;
using Tests.Fakes;
using StrideGenerator.Services;
using System.Collections.Generic;
using StrideGenerator.Data;
using StrideGenerator.Cli;
using System.Linq;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private MemoryStreamReader _memoryStreamReader;
        private readonly IConsole _console = new FakeConsole();
        private ObjReader _objReader;
        private List<MeshObject> _meshes;
        private StridePiece[] _stridePieces;
        private ProcessingPiece[] _processingPieces;
        private int _strideSize;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _memoryStreamReader = new("data/lanhoso.obj");
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _objReader = new ObjReader(_console, _memoryStreamReader);
            _meshes = [];

            _stridePieces = [.. StrideParameterParser.Parse("VT0:F")];//.Where(w => w.AttributeTypesPerInput.Any(x => x.InputIndex == 0)).ToArray();
            _processingPieces = [.. ProcessingParameterParser.Parse("")];//.Where(w => w.InputIndex == 0).ToArray();
            _strideSize = _stridePieces.Sum(x => x.TotalSize);
        }

        [Benchmark]
        public object Scenario1()
        {
            _objReader.ReadInput(_meshes, new InputSettings(), _stridePieces, _processingPieces, false, _strideSize);
            return _meshes;
        }
    }
}
