Feature: OpenAPI Code Generation
Generate C# code from OpenAPI specifications

  Scenario: Generate class from schema type object
    Given the OpenAPI specification:
      """
      openapi: 3.0.0
      info:
        title: Sample API
        version: 1.0.0
      paths: {}
      components:
        schemas:
          Person:
            type: object
            properties:
              Name:
                type: string
              Age:
                type: integer
      """
    When the code generator is executed
    Then the following code should be generated:
      """
      using System;
      
      namespace SampleAPI
      {
        public class Person
          {
            public string Name { get; set; }
      
            public int Age { get; set; }
          }
      }
      """
