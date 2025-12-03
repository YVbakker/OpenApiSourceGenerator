using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace OpenApiSourceGenerator.Tests;

public static class SyntaxDiffExtensions
{
    public static SyntaxDiff? FindFirstDiff(this SyntaxNode leftRoot, SyntaxNode rightRoot)
    {
        return FindFirstDiffInternal(leftRoot, rightRoot);
    }

    private static SyntaxDiff? FindFirstDiffInternal(SyntaxNodeOrToken left, SyntaxNodeOrToken right)
    {
        // Different node/token kind? That's an immediate difference.
        if (left.Kind() != right.Kind())
        {
            return MakeDiff(left, right, $"Different kinds: {left.Kind()} vs {right.Kind()}");
        }

        // If both are tokens, compare "meaningful" properties, ignoring trivia.
        if (left.IsToken && right.IsToken)
        {
            var lt = left.AsToken();
            var rt = right.AsToken();

            // Ignore leading/trailing trivia; look at kind and ValueText.
            if (lt.Kind() != rt.Kind() || lt.ValueText != rt.ValueText)
            {
                return MakeDiff(left, right, $"Different tokens: '{lt.ValueText}' vs '{rt.ValueText}'");
            }

            return null; // tokens are equivalent
        }

        // Both are nodes: compare children.
        var leftChildren  = left.ChildNodesAndTokens();
        var rightChildren = right.ChildNodesAndTokens();

        if (leftChildren.Count != rightChildren.Count)
        {
            return MakeDiff(
                left,
                right,
                $"Different child count: {leftChildren.Count} vs {rightChildren.Count}");
        }

        for (int i = 0; i < leftChildren.Count; i++)
        {
            var lc = leftChildren[i];
            var rc = rightChildren[i];

            if (lc.IsEquivalentTo(rc))
                continue;

            var deeper = FindFirstDiffInternal(lc, rc);
            return deeper ?? MakeDiff(lc, rc, "Subtree not equivalent");
        }

        return null; // structurally equivalent
    }

    private static SyntaxDiff MakeDiff(SyntaxNodeOrToken left, SyntaxNodeOrToken right, string message)
    {
        // These may be null if the node/token is from a detached tree
        var leftTree  = left.SyntaxTree;
        var rightTree = right.SyntaxTree;

        var leftLoc  = leftTree?.GetLineSpan(left.Span);
        var rightLoc = rightTree?.GetLineSpan(right.Span);

        return new SyntaxDiff(left, right, message, leftLoc, rightLoc);
    }
}