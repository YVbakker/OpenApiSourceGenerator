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
public class SchemaProcessor
{
    private readonly PropertyGenerator _propertyGenerator;
    private readonly ClassGenerator _classGenerator;

    public SchemaProcessor(PropertyGenerator propertyGenerator, ClassGenerator classGenerator)
    {
        _propertyGenerator = propertyGenerator ?? throw new ArgumentNullException(nameof(propertyGenerator));
        _classGenerator = classGenerator ?? throw new ArgumentNullException(nameof(classGenerator));
    }

    public List<CodeGenerationResult> ProcessSchema(
        KeyValuePair<string, IOpenApiSchema> schema, 
        string documentName)
    {
        var results = new List<CodeGenerationResult>();

        if (schema.Value.Type is not JsonSchemaType.Object)
        {
            return results;
        }

        var properties = GenerateProperties(schema, documentName, results);
        var classDeclaration = _classGenerator.GenerateClass(schema.Key, properties);
        var compilationUnit = ClassGenerator.GenerateCompilationUnit(documentName, classDeclaration);
        
        var code = compilationUnit.ToFullString();
        results.Add(new CodeGenerationResult(schema.Key, code));

        return results;
    }

    private IEnumerable<PropertyDeclarationSyntax> GenerateProperties(
        KeyValuePair<string, IOpenApiSchema> schema,
        string documentName,
        List<CodeGenerationResult> results)
    {
        if (schema.Value.Properties is null)
        {
            return Enumerable.Empty<PropertyDeclarationSyntax>();
        }

        return schema.Value.Properties.Select(property =>
        {
            // If the property is a nested object (not a reference), process it recursively
            if (property.Value.Type is JsonSchemaType.Object && property.Value is not OpenApiSchemaReference)
            {
                results.AddRange(ProcessSchema(property, documentName));
            }
            
            return _propertyGenerator.GenerateProperty(property, schema.Value.Required);
        });
    }
}
