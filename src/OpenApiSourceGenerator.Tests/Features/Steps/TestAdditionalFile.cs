using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OpenApiSourceGenerator.Tests.Features.Steps;

/// <summary>
/// Helper class to create additional files for testing source generators
/// </summary>
internal class TestAdditionalFile(string path, string text) : AdditionalText
{
    private readonly string _text = text;

    public override string Path { get; } = path;

    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return SourceText.From(_text);
    }
}
