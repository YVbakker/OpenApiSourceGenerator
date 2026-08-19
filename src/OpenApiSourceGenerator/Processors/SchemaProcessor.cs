using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using OpenApiSourceGenerator.Generators;
using OpenApiSourceGenerator.Model;

namespace OpenApiSourceGenerator.Processors;

/// <summary>
/// Processes OpenAPI schemas and generates code for each schema object
/// </summary>
public class SchemaProcessor(PropertyGenerator propertyGenerator, ClassGenerator classGenerator, EnumGenerator enumGenerator)
{
    private readonly PropertyGenerator _propertyGenerator = propertyGenerator ?? throw new ArgumentNullException(nameof(propertyGenerator));
    private readonly ClassGenerator _classGenerator = classGenerator ?? throw new ArgumentNullException(nameof(classGenerator));
    private readonly EnumGenerator _enumGenerator = enumGenerator ?? throw new ArgumentNullException(nameof(enumGenerator));

    public List<CodeGenerationResult> ProcessSchema(
        string typeName,
        IOpenApiSchema schema,
        string documentName,
        TypeNameAllocator typeNameAllocator)
    {
        var results = new List<CodeGenerationResult>();

        if (EnumGenerator.IsEnumSchema(schema))
        {
            var enumDeclaration = _enumGenerator.GenerateEnum(typeName, schema);
            var enumCompilationUnit = EnumGenerator.GenerateCompilationUnit(documentName, enumDeclaration);
            results.Add(new CodeGenerationResult(typeName, enumCompilationUnit.ToFullString()));

            return results;
        }

        if (schema.Type is not JsonSchemaType.Object)
        {
            return results;
        }

        var properties = GenerateProperties(typeName, schema, documentName, results, typeNameAllocator);
        var classDeclaration = _classGenerator.GenerateClass(typeName, properties);
        var compilationUnit = ClassGenerator.GenerateCompilationUnit(documentName, classDeclaration);
        
        var code = compilationUnit.ToFullString();
        results.Add(new CodeGenerationResult(typeName, code));

        return results;
    }

    private List<PropertyDeclarationSyntax> GenerateProperties(
        string typeName,
        IOpenApiSchema schema,
        string documentName,
        List<CodeGenerationResult> results,
        TypeNameAllocator typeNameAllocator)
    {
        if (schema.Properties is null)
        {
            return [];
        }

        return [.. schema.Properties.Select(property =>
        {
            string? resolvedTypeName = null;

            // Inline objects and enums declare their own type, so they need a name of their own
            if (property.Value is not OpenApiSchemaReference
                && (property.Value.Type is JsonSchemaType.Object || EnumGenerator.IsEnumSchema(property.Value)))
            {
                resolvedTypeName = typeNameAllocator.AllocateNested(typeName, property.Key);
                results.AddRange(ProcessSchema(resolvedTypeName, property.Value, documentName, typeNameAllocator));
            }

            return _propertyGenerator.GenerateProperty(property, schema.Required, resolvedTypeName);
        })];
    }
}
