using System;
using System.Collections.Generic;

namespace OpenApiSourceGenerator.Processors;

/// <summary>
/// Single authority for the C# type names emitted for one document, guaranteeing uniqueness.
/// </summary>
public sealed class TypeNameAllocator
{
    private readonly HashSet<string> _allocated = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _allocatedNamesByPreferredName = new(StringComparer.Ordinal);

    public string Allocate(string preferredName)
    {
        var baseName = Sanitize(preferredName);
        var name = baseName;

        for (var suffix = 2; !_allocated.Add(name); suffix++)
        {
            name = baseName + suffix;
        }

        _allocatedNamesByPreferredName[preferredName] = name;
        return name;
    }

    public string Resolve(string preferredName)
    {
        if (_allocatedNamesByPreferredName.TryGetValue(preferredName, out var name))
        {
            return name;
        }

        throw new InvalidOperationException($"No type name has been allocated for '{preferredName}'.");
    }

    public string AllocateNested(string containingTypeName, string propertyName)
    {
        return Allocate(containingTypeName + propertyName.ToPascalCase());
    }

    private static string Sanitize(string preferredName)
    {
        var name = preferredName.ToPascalCase();

        if (name.Length == 0)
        {
            return "GeneratedType";
        }

        return char.IsDigit(name[0]) ? "Type" + name : name;
    }
}
