using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using OpenApiSourceGenerator;
using Reqnroll;
using Shouldly;

namespace OpenApiSourceGenerator.Tests.Features.Steps;

[Binding]
public sealed class FileFormatsStepDefinitions(ScenarioContext scenarioContext)
{
    private const string MinimalYamlSpec = """
        openapi: 3.0.0
        info:
          title: API
          version: 1.0.0
        paths: {}
        components:
          schemas:
            Model:
              type: object
              properties:
                Name:
                  type: string
        """;

    private const string MinimalJsonSpec = """
        {
          "openapi": "3.0.0",
          "info": {"title": "API", "version": "1.0.0"},
          "paths": {},
          "components": {
            "schemas": {
              "Model": {
                "type": "object",
                "properties": {
                  "Name": {"type": "string"}
                }
              }
            }
          }
        }
        """;

    [When("the code generator is executed with file extension {word}")]
    public void WhenTheCodeGeneratorIsExecutedWithFileExtension(string extension)
    {
        var specification = extension == ".json" ? MinimalJsonSpec : MinimalYamlSpec;

        var generator = new OpenApiSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add the OpenAPI specification as an additional file with the correct extension
        driver = driver.AddAdditionalTexts(
            [new TestAdditionalFile($"test{extension}", specification)]
        );

        // To run generators, we can use an empty compilation
        var compilation = CSharpCompilation.Create(nameof(FileFormatsStepDefinitions));

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        scenarioContext["GeneratedCompilation"] = newCompilation;
        scenarioContext["Diagnostics"] = diagnostics;
    }

    [Then("code should be generated")]
    public void ThenCodeShouldBeGenerated()
    {
        var newCompilation = (Compilation)scenarioContext["GeneratedCompilation"];
        
        // Get all generated syntax trees (excluding the empty compilation)
        var generatedTrees = newCompilation.SyntaxTrees
            .Where(t => !string.IsNullOrEmpty(t.FilePath))
            .ToList();

        // Verify that code was generated
        generatedTrees.ShouldNotBeEmpty("No code was generated");
    }
}
