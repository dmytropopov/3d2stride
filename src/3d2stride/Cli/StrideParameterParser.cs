using StrideGenerator.Data;
using StrideGenerator.Services;
using System.Text.RegularExpressions;

namespace StrideGenerator.Cli;

public static partial class StrideParameterParser
{
    [GeneratedRegex("([a-zA-Z]*)(\\d*)")]
    private static partial Regex AttributeRegex();

    public static List<StridePiece> Parse(string input)
    {
        input = input.ToUpperInvariant();
        var result = new List<StridePiece>();
        int offset = 0;

        foreach (var part in input.Split(','))
        {
            var subparts = part.Split(':');
            var attribute = subparts[0].ToUpperInvariant();
            var format = subparts[1].ToUpperInvariant();

            var matchGroups = AttributeRegex().Matches(attribute)[0].Groups;
            if (matchGroups.Count == 3)
            {
                if (CliConstants.AttributeInfos.TryGetValue(matchGroups[1].Value, out var attributeInfo))
                {
                    var index = int.Parse(matchGroups[2].Value);

                    if (!CliConstants.FormatInfos.TryGetValue(format, out var attributeFormat))
                    {
                        throw new NotImplementedException("Output format not recognized: " + format);
                    }

                    if (attributeInfo.TransformsTo == null)
                    {
                        AddAttribute(result, attributeInfo.AttributeComponentType, index, attributeFormat, ref offset);
                    }
                    else
                    {
                        foreach (var transform in attributeInfo.TransformsTo)
                        {
                            AddAttribute(result, CliConstants.AttributeInfos[transform].AttributeComponentType, index, attributeFormat, ref offset);
                        }
                    }
                }
                else
                {
                    throw new Exception("Output attribute not recognized: " + matchGroups[1].Value);
                }
            }
            else
            {
                throw new Exception("Invalid format of attribute: " + attribute);
            }
        }

        return result;
    }

    private static void AddAttribute(List<StridePiece> result, AttributeComponentType attributeComponentType, int inputIndex, (AttributeFormat AttributeFormat, string HelpText) attributeFormat, ref int offset)
    {
        var sizeInBytes = Constants.FormatSizes[attributeFormat.AttributeFormat];
        result.Add(new(attributeFormat.AttributeFormat, sizeInBytes, inputIndex, offset, [attributeComponentType /*bit-packed attribute formats not parsed yet*/ ]));
        offset += sizeInBytes;
    }
}
