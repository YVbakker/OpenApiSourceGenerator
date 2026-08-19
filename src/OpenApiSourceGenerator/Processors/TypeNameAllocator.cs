using System;
using System.Collections.Generic;

namespace OpenApiSourceGenerator.Processors;

/// <summary>
/// Single authority for the C# type names emitted for one document, guaranteeing uniqueness.
/// </summary>
public sealed class TypeNameAllocator
{
    private readonly HashSet<string> _allocated = new(StringComparer.Ordinal);

    public string Allocate(string preferredName)
    {
        var baseName = preferredName.ToPascalCase();
        var name = baseName;

        for (var suffix = 2; !_allocated.Add(name); suffix++)
        {
            name = baseName + suffix;
        }

        return name;
    }

    public string AllocateNested(string containingTypeName, string propertyName)
    {
        return Allocate(containingTypeName + propertyName.ToPascalCase());
    }
}
