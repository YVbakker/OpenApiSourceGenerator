using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenApiSourceGenerator.Generators;

/// <summary>
/// Generates C# class declarations from OpenAPI schemas
/// </summary>
public class ClassGenerator
{
    private readonly PropertyGenerator _propertyGenerator;

    public ClassGenerator(PropertyGenerator propertyGenerator)
    {
        _propertyGenerator = propertyGenerator ?? throw new ArgumentNullException(nameof(propertyGenerator));
    }

    public ClassDeclarationSyntax GenerateClass(
        string className,
        IEnumerable<PropertyDeclarationSyntax> properties)
    {
        return ClassDeclaration(className.ToPascalCase())
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers(properties.ToArray<MemberDeclarationSyntax>());
    }

    public static CompilationUnitSyntax GenerateCompilationUnit(
        string namespaceName,
        ClassDeclarationSyntax classDeclaration)
    {
        var namespaceDeclaration = NamespaceDeclaration(IdentifierName(namespaceName.ToPascalCase()))
            .AddMembers(classDeclaration);

        return CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System")))
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();
    }
}
