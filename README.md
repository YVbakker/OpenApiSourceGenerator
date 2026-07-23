[![CI](https://github.com/YVbakker/OpenApiSourceGenerator/actions/workflows/ci.yml/badge.svg)](https://github.com/YVbakker/OpenApiSourceGenerator/actions/workflows/ci.yml) [![codecov](https://codecov.io/github/YVbakker/OpenApiSourceGenerator/graph/badge.svg?token=ZV2DNEXJDS)](https://codecov.io/github/YVbakker/OpenApiSourceGenerator)

# OpenApiSourceGenerator

A straightforward C# source generator that generates strongly-typed C# classes from OpenAPI specifications (OpenAPI 3.x) at compile time. No additional build steps or tools required!

## Features

- ✨ **Compile-time code generation** - Classes are generated automagically during build
- 🚀 **Zero runtime overhead** - All code generation happens at compile time
- 📝 **OpenAPI 3.x support** - Reads YAML and JSON OpenAPI specifications
- Built on top of Microsoft.OpenApi
- System.Text.Json compatible

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package OpenApiSourceGenerator
```

Or via Package Manager Console:

```powershell
Install-Package OpenApiSourceGenerator
```

## Getting Started

1. **Add your OpenAPI specification file** to your project (YAML or JSON format):
   ```
   openapi/
   └── openapi.yaml
   ```

2. **Ensure the file is included** in your project with `AdditionalFiles`:
   ```xml
   <ItemGroup>
     <AdditionalFiles Include="openapi\openapi.yaml" />
   </ItemGroup>
   ```

3. **Build your project** - Classes will be automatically generated!

## Usage Example

Given an OpenAPI specification:

```yaml
openapi: 3.0.0
info:
  title: Pet Store API
  version: 1.0.0
components:
  schemas:
    Pet:
      type: object
      properties:
        id:
          type: integer
        name:
          type: string
        status:
          type: string
```

The generator will create:

```csharp
public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
}
```

Use the generated classes in your code:

```csharp
var pet = new Pet
{
    Id = 1,
    Name = "Fluffy",
    Status = "available"
};
```

## Known Limitations

The following OpenAPI features are currently **not supported** or only **partially supported**. Schemas that use these features will either be silently skipped or cause a build-time `NotImplementedException`, which surfaces as a Roslyn generator diagnostic (`CS8785`).

### Feature Support Matrix

| Feature | Status | Notes |
|---|---|---|
| Object schemas with primitive/object/array properties | ✅ Supported | Core functionality |
| `$ref` references | ✅ Supported | Object and array item references |
| `required` properties | ✅ Supported | Emits C# `required` modifier |
| `format` | ⚠️ Not supported | `format` values are ignored; all `integer` fields map to `int` (e.g. `int64` stays `int`, not `long`) and format modifiers for `string` fields (e.g. `date-time`, `uuid`, `byte`) are ignored, leaving the type as `string`. See [#87](https://github.com/YVbakker/OpenApiSourceGenerator/issues/87) |
| `enum` | ❌ Not supported | Enum schemas are silently skipped; no C# `enum` type is generated. See [#90](https://github.com/YVbakker/OpenApiSourceGenerator/issues/90) |
| `nullable` / OAS 3.1 null unions | ❌ Not supported | `type: null` or union types including `null` throw a `NotImplementedException`. See [#88](https://github.com/YVbakker/OpenApiSourceGenerator/issues/88) |
| `oneOf` / `anyOf` / `allOf` | ❌ Not supported | Composition keywords are not handled and are silently skipped. See [#91](https://github.com/YVbakker/OpenApiSourceGenerator/issues/91) |
| `additionalProperties` | ❌ Not supported | Dictionary/map schemas are not generated. See [#89](https://github.com/YVbakker/OpenApiSourceGenerator/issues/89) |
| Non-object top-level schemas | ⚠️ Skipped | Only top-level schemas with `type: object` generate a C# class; other types are silently skipped |
| Array schemas without `items` | ❌ Not supported | Throws `NotImplementedException` at build time |

### NotImplementedException Scenarios

The following schema patterns will cause the generator to throw a `NotImplementedException` during compilation, which surfaces as a `CS8785` diagnostic:

- A property or schema with `type: null`
- An array property (`type: array`) without an `items` definition
- An array `items` schema that is not a primitive type or an object reference (e.g. `items: { type: array }`)

### Workarounds

Until full support is added, consider the following workarounds:

- **`format`**: If you need precise types such as `long`, `DateTimeOffset`, or `Guid`, manually add the generated partial class with the correct property type in your project, or post-process the generated code.
- **`enum`**: Define the enum type manually in your project and add the generated partial class to reference it.
- **`nullable`**: Avoid using `type: null` or OAS 3.1 null union types in schemas until support is added in [#88](https://github.com/YVbakker/OpenApiSourceGenerator/issues/88).
- **`oneOf`/`anyOf`/`allOf`**: Replace composition with explicit property duplication in your spec, or define types manually.
- **`additionalProperties`**: Define dictionary properties manually.
- **Build-time failures**: If the generator encounters an unsupported schema, it throws a `NotImplementedException` that surfaces as a `CS8785` diagnostic, failing the build. Remove or exclude the problematic schema(s) from your spec until support is available.

## Configuration

Place OpenAPI files in your project and mark them as `AdditionalFiles`:

```xml
<ItemGroup>
  <AdditionalFiles Include="**\*.yaml" />
  <AdditionalFiles Include="**\openapi.json" />
</ItemGroup>
```

## Requirements

- .NET Standard 2.0 or higher
- C# 7.3 or higher

## How It Works

This package uses [Roslyn Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) to analyze OpenAPI specification files during compilation and generate corresponding C# classes. The generated code is added directly to your compilation, providing IntelliSense support and compile-time type checking.

## Support

- 📖 [Documentation](https://github.com/YVbakker/OpenApiSourceGenerator)
- 🐛 [Report Issues](https://github.com/YVbakker/OpenApiSourceGenerator/issues)
- 💬 [Discussions](https://github.com/YVbakker/OpenApiSourceGenerator/discussions)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.