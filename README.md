# StaticCodeAnalysis

Static code analysis for a C# file using .NET Compiler Platform ("Roslyn")

## Structure

| Project                 | Explanation                                                                             |
|-------------------------|-----------------------------------------------------------------------------------------|
| ExampleCode             | contains a .cs file with several (derived) classes and (overridden) methods for testing |
| ExampleCodeTests        | test project for ExampleCode for gaining coverage results with coverlet                 |
| StaticCodeAnalysis      | actual code analysis                                                                    |
| StaticCodeAnalysisTests | test project for StaticCodeAnalysis using ExampleCode                                   |

## Usage

Just clone or download this repo, open in Visual Studio, adjust paths to the ExampleCode.cs in
 * ```StaticCodeAnalysis/StaticCodeAnalysis.cs``` and
 * ```StaticCodeAnalysis/StaticCodeAnalysisTests.cs```

and execute all tests to observe behaviour.

## General Information

|                |            |
|----------------|------------|
| Author         | Felix Keck |
| .NET Framework | 4.6.1      |
| Visual Studio  | 2017       |
