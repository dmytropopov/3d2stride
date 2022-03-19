using Microsoft.Extensions.Logging;

namespace StrideGenerator.Services;

public class Generator : IGenerator
{
    private readonly ILogger _logger;

    public Generator(ILogger<Generator> logger)
    {
        _logger = logger;
    }

    public async Task Generate()
    {
        _logger.LogInformation("Generate();");
    }
}
