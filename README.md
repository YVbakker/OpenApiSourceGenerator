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
          format: int64
        name:
          type: string
        status:
          type: string
```

The generator will create:

```csharp
public class Pet
{
    public long Id { get; set; }
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

- 📖 [Documentation](https://github.com/yourusername/OpenApiSourceGenerator)
- 🐛 [Report Issues](https://github.com/yourusername/OpenApiSourceGenerator/issues)
- 💬 [Discussions](https://github.com/yourusername/OpenApiSourceGenerator/discussions)

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.