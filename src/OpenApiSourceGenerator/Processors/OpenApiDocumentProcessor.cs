using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;
using OpenApiSourceGenerator.Generators;
using OpenApiSourceGenerator.Model;

namespace OpenApiSourceGenerator.Processors;

/// <summary>
/// Processes OpenAPI documents and orchestrates code generation
/// </summary>
public class OpenApiDocumentProcessor
{
    private readonly SchemaProcessor _schemaProcessor;

    public OpenApiDocumentProcessor(SchemaProcessor schemaProcessor)
    {
        _schemaProcessor = schemaProcessor ?? throw new ArgumentNullException(nameof(schemaProcessor));
    }

    public IReadOnlyList<CodeGenerationResult> ProcessDocument(OpenApiDocument document)
    {
        var documentName = document.Info.Title ?? "GeneratedClasses";
        var schemas = document.Components?.Schemas ?? throw new InvalidOperationException("Document has no schemas");
        var typeNameAllocator = new TypeNameAllocator();

        // Materialized so every component claims its name before any inline type is named
        var declaredSchemas = schemas
            .Where(schema => schema.Value.Type is JsonSchemaType.Object || EnumGenerator.IsEnumSchema(schema.Value))
            .Select(schema => (TypeName: typeNameAllocator.Allocate(schema.Key), schema.Value))
            .ToList();

        return [.. declaredSchemas.SelectMany(schema =>
            _schemaProcessor.ProcessSchema(schema.TypeName, schema.Value, documentName, typeNameAllocator))];
    }
}
