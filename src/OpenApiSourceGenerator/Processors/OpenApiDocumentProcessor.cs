using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;
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

    public IEnumerable<CodeGenerationResult> ProcessDocument(OpenApiDocument document)
    {
        var documentName = document.Info.Title ?? "GeneratedClasses";

        return (document.Components?.Schemas ?? throw new InvalidOperationException("Document has no schemas"))
            .Where(schema => schema.Value.Type is JsonSchemaType.Object)
            .SelectMany(schema => _schemaProcessor.ProcessSchema(schema, documentName));
    }
}
