using System;
using System.Collections.Generic;

namespace OpenApiSourceGenerator.Processors;

/// <summary>
/// Single authority for the C# type names emitted for one document, guaranteeing uniqueness.
/// </summary>
public sealed class TypeNameAllocator
{
    private readonly HashSet<string> _allocated = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _allocatedNamesByComponentKey = new(StringComparer.Ordinal);

    public string Allocate(string componentKey)
    {
        var name = AllocateName(componentKey);

        // Only component keys are resolvable via $ref; nested/inline names must never overwrite these entries.
        _allocatedNamesByComponentKey[componentKey] = name;
        return name;
    }

    public string Resolve(string componentKey)
    {
        if (_allocatedNamesByComponentKey.TryGetValue(componentKey, out var name))
        {
            return name;
        }

        throw new InvalidOperationException($"No type name has been allocated for '{componentKey}'.");
    }

    public string AllocateNested(string containingTypeName, string propertyName)
    {
        return AllocateName(containingTypeName + propertyName.ToPascalCase());
    }

    private string AllocateName(string preferredName)
    {
        var baseName = Sanitize(preferredName);
        var name = baseName;

        for (var suffix = 2; !_allocated.Add(name); suffix++)
        {
            name = baseName + suffix;
        }

        return name;
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
