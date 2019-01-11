# StaticCodeAnalysis

Static code analysis for a C# file using .NET Compiler Platform ("Roslyn")

## Structure

| Project                 | Explanation                                                                             |
|-------------------------|-----------------------------------------------------------------------------------------|
| ExampleCode             | contains a .cs file with several (derived) classes and (overridden) methods for testing |
| ExampleCodeTests        | test project for ExampleCode for gaining coverage results with coverlet                 |
| StaticCodeAnalysis      | actual code analysis                                                                    |
| StaticCodeAnalysisTests | test project for StaticCodeAnalysis using ExampleCode                                   |

## Requirements

The following Visual Studio Workloads are neccessary:
 * .NET desktop development with
   * .NET framework development tools
   * .NET Core development tools
 * Visual Studio extension development with
   * .NET Compiler Platform SDK

## General Information

|                |            |
|----------------|------------|
| Author         | Felix Keck |
| .NET Framework | 4.6.1      |
| Visual Studio  | 2017       |
