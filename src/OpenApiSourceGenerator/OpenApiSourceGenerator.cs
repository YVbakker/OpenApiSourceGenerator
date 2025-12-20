using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using OpenApiSourceGenerator.Generators;
using OpenApiSourceGenerator.Processors;

namespace OpenApiSourceGenerator;

/// <summary>
/// Roslyn source generator that generates C# classes from OpenAPI YAML files
/// </summary>
[Generator]
public class OpenApiSourceGenerator : IIncrementalGenerator
{
    private readonly OpenApiReaderSettings _openApiReaderSettings = new();
    private readonly OpenApiDocumentProcessor _documentProcessor;

    public OpenApiSourceGenerator() : this(CreateDocumentProcessor())
    {
    }

    internal OpenApiSourceGenerator(OpenApiDocumentProcessor documentProcessor)
    {
        _documentProcessor = documentProcessor;
    }

    private static OpenApiDocumentProcessor CreateDocumentProcessor()
    {
        var propertyGenerator = new PropertyGenerator();
        var classGenerator = new ClassGenerator(propertyGenerator);
        var schemaProcessor = new SchemaProcessor(propertyGenerator, classGenerator);
        return new OpenApiDocumentProcessor(schemaProcessor);
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _openApiReaderSettings.AddYamlReader();
        _openApiReaderSettings.AddJsonReader();
        
        var provider = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) ||
                           file.Path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) ||
                           file.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Collect();

        context.RegisterSourceOutput(provider, GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context, ImmutableArray<AdditionalText> files)
    {
        foreach (var file in files)
        {
            var text = file.GetText();
            if (text is null)
            {
                continue;
            }

            var format = file.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? "json" : "yaml";
            var (openApiDocument, _) = OpenApiDocument.Parse(text.ToString(), format, _openApiReaderSettings);

            if (openApiDocument is null)
            {
                continue;
            }

            foreach (var codeGenerationResult in _documentProcessor.ProcessDocument(openApiDocument))
            {
                context.AddSource($"{codeGenerationResult.Name}.g.cs", codeGenerationResult.Code);
            }
        }
    }
}