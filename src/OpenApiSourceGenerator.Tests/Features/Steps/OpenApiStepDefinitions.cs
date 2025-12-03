using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using OpenApiSourceGenerator;
using Reqnroll;
using Shouldly;

namespace OpenApiSourceGenerator.Tests.Features.Steps;

[Binding]
public sealed class OpenApiStepDefinitions(ScenarioContext scenarioContext)
{
    // For additional details on Reqnroll step definitions see https://go.reqnroll.net/doc-stepdef

    [Given("the OpenAPI specification:")]
    public void GivenTheOpenAPISpecification(string specification)
    {
        scenarioContext["OpenAPISpecification"] = specification;
    }

    [When("the code generator is executed")]
    public void WhenTheCodeGeneratorIsExecuted()
    {
        var specification = (string)scenarioContext["OpenAPISpecification"];

        // Create an instance of the source generator
        var generator = new OpenApiSourceGenerator();

        // Source generators should be tested using 'GeneratorDriver'
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add the OpenAPI specification as an additional file
        driver = driver.AddAdditionalTexts(
            [new TestAdditionalFile("test.yaml", specification)]
        );

        // To run generators, we can use an empty compilation
        var compilation = CSharpCompilation.Create(nameof(OpenApiStepDefinitions));

        // Run generators and get the new compilation
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        // Store the generated code in the scenario context
        scenarioContext["GeneratedCompilation"] = newCompilation;
        scenarioContext["Diagnostics"] = diagnostics;
    }

    [Then("the following code should be generated:")]
    public void ThenTheFollowingCodeShouldBeGenerated(string expectedCode)
    {
        var newCompilation = (Compilation)scenarioContext["GeneratedCompilation"];
        
        // Get all generated syntax trees (excluding the empty compilation)
        var generatedTrees = newCompilation.SyntaxTrees
            .Where(t => !string.IsNullOrEmpty(t.FilePath))
            .ToList();

        // Verify that code was generated
        if (generatedTrees.Count == 0)
        {
            throw new Exception("No code was generated");
        }

        // Get the generated code and normalize it
        var generatedCode = generatedTrees.First().ToString();
        
        // Parse both into syntax trees
        var generatedTree = CSharpSyntaxTree.ParseText(generatedCode);
        var expectedTree = CSharpSyntaxTree.ParseText(expectedCode);

        // Normalize both trees by reformatting them (this removes whitespace/indentation differences)
        var generatedRoot = generatedTree.GetRoot().NormalizeWhitespace();
        var expectedRoot = expectedTree.GetRoot().NormalizeWhitespace();
        
        // Use the custom diff to find differences (if any)
        var diff = generatedRoot.FindFirstDiff(expectedRoot);
        
        if (diff != null)
        {
            // Provide detailed error message if there's a difference
            var message = $"Syntax trees differ: {diff.Message}\n" +
                          $"Generated: {diff.Left} at {diff.LeftLocation}\n" +
                          $"Expected: {diff.Right} at {diff.RightLocation}\n\n" +
                          $"Generated code:\n{generatedRoot.ToFullString()}\n\n" +
                          $"Expected code:\n{expectedRoot.ToFullString()}";
            throw new Exception(message);
        }
        
        // Final equivalence check
        generatedRoot.IsEquivalentTo(expectedRoot).ShouldBeTrue();
    }
}

// Helper class to create additional files for testing
internal class TestAdditionalFile : AdditionalText
{
    private readonly string _text;

    public TestAdditionalFile(string path, string text)
    {
        Path = path;
        _text = text;
    }

    public override string Path { get; }

    public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default)
    {
        return SourceText.From(_text);
    }
}
