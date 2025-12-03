using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi;

namespace OpenApiSourceGenerator.Mappers;

/// <summary>
/// Maps OpenAPI/JSON Schema types to C# syntax kinds
/// </summary>
public static class TypeMapper
{
    public static SyntaxKind GetPrimitiveSyntaxKind(JsonSchemaType? type)
    {
        return type switch
        {
            JsonSchemaType.Boolean => SyntaxKind.BoolKeyword,
            JsonSchemaType.Integer => SyntaxKind.IntKeyword,
            JsonSchemaType.Number => SyntaxKind.DoubleKeyword,
            JsonSchemaType.String => SyntaxKind.StringKeyword,
            JsonSchemaType.Object or JsonSchemaType.Array or JsonSchemaType.Null => 
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
