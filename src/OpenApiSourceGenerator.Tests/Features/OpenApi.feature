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
    Then the following code should be generated for the class Person:
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
    
  Scenario: Generate array type property
    Given the OpenAPI specification:
    """
    openapi: 3.0.0
    info:
      title: Sample API
      version: 1.0.0
    paths: {}
    components:
      schemas:
        Category:
          type: object
          properties:
            id:
              type: integer
              format: int64
              example: 1
            name:
              type: string
              example: Dogs
          xml:
            name: category
        Tag:
          type: object
          properties:
            id:
              type: integer
              format: int64
            name:
              type: string
          xml:
            name: tag
        Pet:
          required:
          - name
          - photoUrls
          type: object
          properties:
            id:
              type: integer
              format: int64
              example: 10
            name:
              type: string
              example: doggie
            category:
              $ref: '#/components/schemas/Category'
            photoUrls:
              type: array
              xml:
                wrapped: true
              items:
                type: string
                xml:
                  name: photoUrl
            tags:
              type: array
              xml:
                wrapped: true
              items:
                $ref: '#/components/schemas/Tag'
            status:
              type: string
              description: pet status in the store
              enum:
              - available
              - pending
              - sold
          xml:
            name: pet
    """
    When the code generator is executed
    Then the following code should be generated for the class Pet:
    """
    using System;
    using System.Collections.Generic;
    
    namespace SampleAPI
    {
        public class Pet
        {
            public int id { get; set; }
            required public string name { get; set; }
            public Category category { get; set; }
            required public List<string> photoUrls { get; set; }
            public List<Tag> tags { get; set; }
            public string status { get; set; }
        }
    }
    """
