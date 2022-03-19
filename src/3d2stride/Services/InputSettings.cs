namespace StrideGenerator.Services;

public readonly record struct InputAttributes(string[] Attributes);

public readonly record struct OutputAttributes(string[] Attributes);

public readonly record struct InputSettings(string FileName, string FileFormat, InputAttributes InputAttributes);

public readonly record struct OutputSettings(string FileName, OutputAttributes OutputAttributes);