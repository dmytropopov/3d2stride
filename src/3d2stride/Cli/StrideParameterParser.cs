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
            var attributeComponents = subparts[0].ToUpperInvariant().Split("+");
            var format = subparts[1].ToUpperInvariant();
            bool attributesAdded = false;
            int dataPosition = 0;

            var attributeComponentTypesPerInput = new List<FormatComponent>();

            if (!CliConstants.FormatInfos.TryGetValue(format, out var attributeFormat))
            {
                throw new NotImplementedException("Output format not recognized: " + format);
            }

            foreach (var attributeComponent in attributeComponents)
            {
                var matchGroups = AttributeRegex().Matches(attributeComponent)[0].Groups;

                if (matchGroups.Count > 1)
                {
                    if (CliConstants.AttributeInfos.TryGetValue(matchGroups[1].Value, out var attributeInfo))
                    {
                        int index = ParseIndex(attributeComponent, matchGroups, attributeInfo);

                        if (attributeInfo.TransformsTo == null)
                        {
                            attributeComponentTypesPerInput.Add(new(index, dataPosition++, attributeInfo.AttributeComponentType));
                        }
                        else
                        {
                            foreach (var transform in attributeInfo.TransformsTo)
                            {
                                if (attributeFormat.RequiredNumberOfComponents > 1)
                                {
                                    throw new Exception($"Bit-packed format {attributeFormat.AttributeFormat} does not allow shorthand attributes notation, use separate components like X, Y, Z.");
                                }
                                AddAttribute(result, [new(index, 0, CliConstants.AttributeInfos[transform].AttributeComponentType)], attributeFormat, ref offset);
                                attributesAdded = true;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Output attribute not recognized: " + matchGroups[1].Value);
                    }
                }
            }

            if (!attributesAdded)
            {
                if (attributeFormat.RequiredNumberOfComponents > 1 && attributeComponentTypesPerInput.Count != attributeFormat.RequiredNumberOfComponents)
                {
                    throw new Exception($"Bit-packed format {attributeFormat.AttributeFormat} requires {attributeFormat.RequiredNumberOfComponents} components exactly. Use empty E if you don't need some.");
                }
                AddAttribute(result, attributeComponentTypesPerInput, attributeFormat, ref offset);
            }
        }

        return result;
    }

    private static int ParseIndex(string attributeComponent, GroupCollection matchGroups, (AttributeComponentType AttributeComponentType, AttributeType AttributeType, int DataIndex, string[]? TransformsTo, string HelpText) attributeInfo)
    {
        int index;
        if (attributeInfo.AttributeComponentType == AttributeComponentType.Empty)
        {
            index = 0;
        }
        else if (matchGroups.Count == 3)
        {
            index = int.Parse(matchGroups[2].Value);
        }
        else
        {
            throw new Exception("Invalid format of attribute component (missing input index): " + attributeComponent);
        }

        return index;
    }

    private static void AddAttribute(List<StridePiece> result, List<FormatComponent> attributeComponentTypesPerInput, (AttributeFormat AttributeFormat, string HelpText, int RequiredNumberOfComponents) attributeFormat, ref int offset)
    {
        var sizeInBytes = Constants.FormatSizes[attributeFormat.AttributeFormat];
        result.Add(new(attributeFormat.AttributeFormat, sizeInBytes, offset, attributeComponentTypesPerInput));
        offset += sizeInBytes;
    }
}
