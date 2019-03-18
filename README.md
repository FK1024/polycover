# OOEdgeCoverage

OOEdgeCoverage is a source code file edge coverage tool for .NET Framework with support for object-oriented features.

## Requirements

* You will need coverlet as global tool which can be installed via
```
dotnet tool install --global coverlet.console
```

* Currently OOEdgeCoverage only supports **one** user-defined method call per line

## Installation

Just clone or download this repo.

## Usage

To see the list of arguments, run:
```
OOEdgeCoverage --help
```

The arguments are:
```
.NET Framework source code edge coverage tool

Arguments:
        <CODEFILE>      Path to the source code file
        <SOLUTION>      Path to the solution
        <TESTPROJECT>   Path to the test project
        <TESTASSEMBLY>  Path to the test assembly
```

An example call could look like following:
```
OOEdgeCoverage "C:\MyApp\MyProject\MyCodeFile.cs" "C:\MyApp\MySolution.sln" "C:\MyApp\MyProjectTests\MyProjectTests.csproj" "C:\MyApp\MyProjectTests\bin\Debug\MyProjectTests.dll"
```

_Note: Mayby you have to add_ `<IsTestProject>true</IsTestProject>` _to your test.csproj file to get the tests run._

## Output

A file named the same as the source code file with "\_YoYoGraph.dgml" appended will be generated in the same directory.

## How It Works

* Analyzes the given source code file using the [.NET Compiler Platform ("Roslyn")](https://github.com/dotnet/roslyn)
* Creates a "YoYo-Graph" from the analyzed code which is basically a call graph with consideration of inherritance and polymorphism
* Modifies the source code file by inserting code to record caller's line numbers
* Rebuilds the entire solution
* Runs [coverlet](https://github.com/tonerdo/coverlet) to check which of the inserted statements were executed
* Calculates the coverage result and marks covered invocations in the YoYo-Graph green, uncovered red
* Restores the original source code file

## General Information

|                |            |
|----------------|------------|
| Author         | Felix Keck |
| .NET Framework | 4.6.1      |
| Visual Studio  | 2017       |
