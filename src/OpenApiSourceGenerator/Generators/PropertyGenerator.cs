using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using OpenApiSourceGenerator.Mappers;
using OpenApiSourceGenerator.Processors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenApiSourceGenerator.Generators;

/// <summary>
/// Generates C# property declarations from OpenAPI schema properties
/// </summary>
public class PropertyGenerator
{
    public PropertyDeclarationSyntax GenerateProperty(
        KeyValuePair<string, IOpenApiSchema> schema,
        ISet<string>? required,
        string? resolvedTypeName = null,
        TypeNameAllocator? typeNameAllocator = null)
    {
        var propertyDeclaration = CreatePropertyDeclaration(schema, resolvedTypeName, typeNameAllocator);

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

    private static PropertyDeclarationSyntax CreatePropertyDeclaration(
        KeyValuePair<string, IOpenApiSchema> schema,
        string? resolvedTypeName,
        TypeNameAllocator? typeNameAllocator)
    {
        var name = schema.Key;
        var type = CreateTypeSyntax(schema.Value, name, resolvedTypeName, typeNameAllocator);

        return PropertyDeclaration(type, Identifier(name));
    }

    private static TypeSyntax CreateTypeSyntax(
        IOpenApiSchema schema,
        string name,
        string? resolvedTypeName,
        TypeNameAllocator? typeNameAllocator)
    {
        if (schema is OpenApiSchemaReference schemaReference
            && (schemaReference.Type is JsonSchemaType.Object || EnumGenerator.IsEnumSchema(schemaReference)))
        {
            var referenceId = schemaReference.Reference.Id ?? schemaReference.Reference.ReferenceV3 ?? name;
            return ParseTypeName(typeNameAllocator?.Resolve(referenceId) ?? referenceId.ToPascalCase());
        }

        if (resolvedTypeName is not null)
        {
            return ParseTypeName(resolvedTypeName);
        }

        return schema.Type switch
        {
            null => throw new NotImplementedException("Schema type cannot be null"),
            JsonSchemaType.Null => throw new NotImplementedException("Null types are not yet supported"),

            var t when TypeMapper.IsPrimitiveType(t) =>
                TypeMapper.GetPrimitiveTypeSyntax(t, schema.Format),

            JsonSchemaType.Array =>
                ListOf(CreateArrayItemTypeSyntax(
                    schema.Items ?? throw new NotImplementedException("Array schema must have items defined"),
                    typeNameAllocator)),

            _ => throw new ArgumentOutOfRangeException(nameof(schema.Type), schema.Type, "Unknown schema type")
        };
    }

    private static TypeSyntax CreateArrayItemTypeSyntax(IOpenApiSchema items, TypeNameAllocator? typeNameAllocator)
    {
        if (items is OpenApiSchemaReference itemReference
            && (itemReference.Type is JsonSchemaType.Object || EnumGenerator.IsEnumSchema(itemReference)))
        {
            var referenceId = itemReference.Reference.Id
                ?? itemReference.Reference.ReferenceV3
                ?? throw new InvalidOperationException("Reference of array type is null");
            return ParseTypeName(typeNameAllocator?.Resolve(referenceId) ?? referenceId.ToPascalCase());
        }

        if (items.Type is null)
            throw new NotImplementedException("Schema type cannot be null");

        return items.Type switch
        {
            var t when TypeMapper.IsPrimitiveType(t) =>
                TypeMapper.GetPrimitiveTypeSyntax(t, items.Format),

            JsonSchemaType.Object =>
                ParseTypeName((items.Title ?? throw new InvalidOperationException("Title of array object type is null")).ToPascalCase()),

            _ => throw new NotImplementedException("Only primitive, object and array schema types are supported")
        };
    }

    private static TypeSyntax ListOf(TypeSyntax itemType) =>
        GenericName(Identifier("List"))
            .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(itemType)));

}