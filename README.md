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
          $ref: '#/components/schemas/PetStatus'
    PetStatus:
      type: string
      enum:
      - available
      - pending
      - sold
```

The generator will create:

```csharp
public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; }
    public PetStatus Status { get; set; }
}

public enum PetStatus
{
    Available,
    Pending,
    Sold
}
```

Use the generated classes in your code:

```csharp
var pet = new Pet
{
    Id = 1,
    Name = "Fluffy",
    Status = PetStatus.Available
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
| `enum` | ✅ Supported | String enum values generate named C# enum members; integer enum values generate named members with numeric assignments. Inline property enums generate a separate enum named after the containing schema and property. |
| `format` | ⚠️ Partially supported | Supported `type` + `format` combinations map to more precise C# types (see [Format Support](#format-support) below); unrecognized or missing `format` values deterministically fall back to the base `type` mapping. |
| `nullable` / OAS 3.1 null unions | ❌ Not supported | `type: null` or union types including `null` throw a `NotImplementedException`. See [#88](https://github.com/YVbakker/OpenApiSourceGenerator/issues/88) |
| `oneOf` / `anyOf` / `allOf` | ❌ Not supported | Composition keywords are not handled and are silently skipped. See [#91](https://github.com/YVbakker/OpenApiSourceGenerator/issues/91) |
| `additionalProperties` | ❌ Not supported | Dictionary/map schemas are not generated. See [#89](https://github.com/YVbakker/OpenApiSourceGenerator/issues/89) |
| Non-object top-level schemas | ⚠️ Partially supported | Top-level enum schemas generate C# enums; other non-object schemas are silently skipped |
| Array schemas without `items` | ❌ Not supported | Throws `NotImplementedException` at build time |

### Enum Generation

OpenAPI enum schemas generate C# `enum` declarations. Top-level component enums use the schema name, referenced enum properties use that generated type, and inline property enums generate a separate enum named after the containing schema and property, such as `PetStatus` for a `status` enum inside `Pet`. If that name is already used by another generated type, a numeric suffix is appended, such as `PetStatus2`.

String enum values are converted to PascalCase member names by removing non-alphanumeric separators. If multiple values normalize to the same member name, a numeric suffix is appended to keep names stable and unique. Non-negative integer enum values generate members named `Value{number}`, while negative values use `Negative{magnitude}`; all integer members have explicit numeric assignments. For example, `5` becomes `Value5 = 5` and `-5` becomes `Negative5 = -5`; values outside the `int` range use a `long` enum backing type.

Generated string enums use `System.Text.Json.Serialization.JsonStringEnumConverter` with `JsonStringEnumMemberNameAttribute` to preserve the original OpenAPI wire values.

Nullable enum schemas follow the current nullable limitation: generated enum properties are not emitted as nullable, and OAS 3.1 null unions remain unsupported.

### Format Support

The following `type` + `format` combinations map to more precise C# types. Any other `format` value (including none) falls back to the base `type` mapping shown in parentheses.

| `type` | `format` | C# type |
|---|---|---|
| `integer` | `int32` (or unrecognized/none) | `int` |
| `integer` | `int64` | `long` |
| `number` | `float` | `float` |
| `number` | `double` (or unrecognized/none) | `double` |
| `string` | `date-time` | `DateTimeOffset` |
| `string` | `uuid` | `Guid` |
| `string` | `byte` | `byte[]` |
| `string` | unrecognized/none | `string` |

### NotImplementedException Scenarios

The following schema patterns will cause the generator to throw a `NotImplementedException` during compilation, which surfaces as a `CS8785` diagnostic:

- A property or schema with `type: null`
- An array property (`type: array`) without an `items` definition
- An array `items` schema that is not a primitive type or an object reference (e.g. `items: { type: array }`)

### Workarounds

Until full support is added, consider the following workarounds:

- **`format`**: For `format` values not listed in [Format Support](#format-support), manually add the generated partial class with the correct property type in your project, or post-process the generated code.
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

- Consuming projects must target .NET 10 or higher.
- The source generator assembly targets .NET Standard 2.0 for compiler/analyzer loadability, but the generated code uses modern .NET APIs.

## How It Works

This package uses [Roslyn Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) and [Microsoft.OpenApi](https://github.com/Microsoft/OpenAPI.NET) to analyze OpenAPI specification files during compilation and generate corresponding C# classes. The generated code is added directly to your compilation, providing IntelliSense support and compile-time type checking.

## Support

- 📖 [Documentation](https://github.com/YVbakker/OpenApiSourceGenerator)
- 🐛 [Report Issues](https://github.com/YVbakker/OpenApiSourceGenerator/issues)
- 💬 [Discussions](https://github.com/YVbakker/OpenApiSourceGenerator/discussions)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
