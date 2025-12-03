using Microsoft.CodeAnalysis;

namespace OpenApiSourceGenerator.Tests;

public sealed record SyntaxDiff(
    SyntaxNodeOrToken Left,
    SyntaxNodeOrToken Right,
    string Message,
    FileLinePositionSpan? LeftLocation = null,
    FileLinePositionSpan? RightLocation = null)
{
    public SyntaxNodeOrToken Left { get; } = Left;
    public SyntaxNodeOrToken Right { get; } = Right;
    public string Message { get; } = Message;
    public FileLinePositionSpan? LeftLocation { get; } = LeftLocation;
    public FileLinePositionSpan? RightLocation { get; } = RightLocation;
}