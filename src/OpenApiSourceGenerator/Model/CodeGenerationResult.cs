namespace OpenApiSourceGenerator.Model;

public class CodeGenerationResult(string name, string code)
{
    public string Name { get; } = name;
    public string Code { get; } = code;
}