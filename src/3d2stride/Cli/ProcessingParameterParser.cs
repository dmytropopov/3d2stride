using StrideGenerator.Services;
using System.Text.RegularExpressions;

namespace StrideGenerator.Cli;

public static partial class ProcessingParameterParser
{
    [GeneratedRegex("([a-zA-Z]*)(\\d*)")]
    private static partial Regex AttributeRegex();

    public static List<ProcessingPiece> Parse(string input)
    {
        var result = new List<ProcessingPiece>();

        if (!string.IsNullOrEmpty(input))
        {
            input = input.ToUpperInvariant();
            foreach (var part in input.Split(','))
            {
                var subparts = part.Split(':');
                var attribute = subparts[0].ToUpperInvariant();
                var processingType = subparts[1].ToUpperInvariant();

                var matchGroups = AttributeRegex().Matches(attribute)[0].Groups;
                if (matchGroups.Count == 3)
                {
                    if (CliConstants.AttributeInfos.TryGetValue(matchGroups[1].Value, out var attributeInfo))
                    {
                        var inputIndex = int.Parse(matchGroups[2].Value);

                        if (!CliConstants.ProcessingInfos.TryGetValue(processingType, out var processingInfo))
                        {
                            throw new NotImplementedException("Procesing type not recognized: " + processingType);
                        }

                        result.Add(new(inputIndex, processingInfo.ProcesingType, attributeInfo.AttributeType, attributeInfo.DataIndex));
                    }
                    else
                    {
                        throw new Exception("Processing attribute not recognized: " + matchGroups[1].Value);
                    }
                }
                else
                {
                    throw new Exception("Invalid format of processing at attribute: " + attribute);
                }
            }
        }

        return result;
    }
}
