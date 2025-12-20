Feature: File Format Support
Verify that different file extensions (.yaml, .yml, .json) are correctly recognized and parsed

  Scenario Outline: Parse OpenAPI file with <extension> extension
    When the code generator is executed with file extension <extension>
    Then code should be generated

    Examples:
      | extension |
      | .yaml     |
      | .yml      |
      | .json     |
