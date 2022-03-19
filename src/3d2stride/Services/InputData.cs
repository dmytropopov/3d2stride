namespace StrideGenerator.Services;

public readonly record struct InputAttributes(string[] Attributes);

public readonly record struct InputData(string FileName, InputAttributes InputAttributes);
