using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenApiSourceGenerator.Mappers;

/// <summary>
/// Maps OpenAPI/JSON Schema type + format combinations to C# type syntax
/// </summary>
public static class TypeMapper
{
    /// <summary>
    /// Resolves the C# type syntax for a primitive JSON schema type, honoring the
    /// OpenAPI <c>format</c> keyword where a supported, more specific mapping exists.
    /// Unknown or unsupported formats deterministically fall back to the base type mapping.
    /// </summary>
    public static TypeSyntax GetPrimitiveTypeSyntax(JsonSchemaType? type, string? format)
    {
        return (type, format) switch
        {
            (JsonSchemaType.Boolean, _) => PredefinedType(Token(SyntaxKind.BoolKeyword)),

            (JsonSchemaType.Integer, "int64") => PredefinedType(Token(SyntaxKind.LongKeyword)),
            (JsonSchemaType.Integer, _) => PredefinedType(Token(SyntaxKind.IntKeyword)),

            (JsonSchemaType.Number, "float") => PredefinedType(Token(SyntaxKind.FloatKeyword)),
            (JsonSchemaType.Number, _) => PredefinedType(Token(SyntaxKind.DoubleKeyword)),

            // Globally qualified so a same-named component schema (e.g. "Guid") cannot shadow the framework type.
            (JsonSchemaType.String, "date-time") => ParseTypeName("global::System.DateTimeOffset"),
            (JsonSchemaType.String, "uuid") => ParseTypeName("global::System.Guid"),
            (JsonSchemaType.String, "byte") => ArrayType(PredefinedType(Token(SyntaxKind.ByteKeyword)))
                .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(
                    SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))),
            (JsonSchemaType.String, _) => PredefinedType(Token(SyntaxKind.StringKeyword)),

            (JsonSchemaType.Object or JsonSchemaType.Array or JsonSchemaType.Null, _) =>
                throw new NotImplementedException($"Type {type} is not a primitive type"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown JSON schema type")
        };
    }

    public static bool IsPrimitiveType(JsonSchemaType? type)
    {
        return type is JsonSchemaType.Boolean 
            or JsonSchemaType.Integer 
            or JsonSchemaType.Number 
            or JsonSchemaType.String;
    }
}
