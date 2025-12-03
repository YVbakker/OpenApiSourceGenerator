using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using OpenApiSourceGenerator.Mappers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenApiSourceGenerator.Generators;

/// <summary>
/// Generates C# property declarations from OpenAPI schema properties
/// </summary>
public class PropertyGenerator
{
    public PropertyDeclarationSyntax GenerateProperty(
        KeyValuePair<string, IOpenApiSchema> schema, 
        ISet<string>? required)
    {
        var propertyDeclaration = CreatePropertyDeclaration(schema);
        
        // Add required modifier if the property is required
        if (required is not null && required.Contains(schema.Key))
        {
            propertyDeclaration = propertyDeclaration.AddModifiers(Token(SyntaxKind.RequiredKeyword));
        }

        return propertyDeclaration
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            );
    }

    private static PropertyDeclarationSyntax CreatePropertyDeclaration(KeyValuePair<string, IOpenApiSchema> schema)
    {
        if (schema.Value is not OpenApiSchemaReference reference || schema.Value.Type is not JsonSchemaType.Object)
        {
            return schema.Value.Type switch
            {
                JsonSchemaType.Null =>
                    throw new NotImplementedException("Null types are not yet supported"),

                var primitiveType when TypeMapper.IsPrimitiveType(primitiveType) =>
                    PropertyDeclaration(
                        PredefinedType(Token(TypeMapper.GetPrimitiveSyntaxKind(primitiveType))),
                        Identifier(schema.Key)),

                JsonSchemaType.Object =>
                    PropertyDeclaration(
                        ParseTypeName(schema.Key.ToPascalCase()),
                        Identifier(schema.Key)),

                JsonSchemaType.Array =>
                    throw new NotImplementedException("Array types are not yet supported"),

                null =>
                    throw new NotImplementedException("Schema type cannot be null"),

                _ => throw new ArgumentOutOfRangeException(nameof(schema.Value.Type), schema.Value.Type, "Unknown schema type")
            };
        }
        var referenceId = reference.Reference.Id ?? reference.Reference.ReferenceV3 ?? schema.Key;
        return PropertyDeclaration(
            ParseTypeName(referenceId.ToPascalCase()), 
            Identifier(schema.Key));
    }
}
