# polycover

Polycover is a source code coverage tool for .NET Framework with focus on polymorphic structures.

## Requirements

* You will need [coverlet](https://github.com/tonerdo/coverlet) as global tool which can be installed via
```
dotnet tool install --global coverlet.console
```

## Limitations

* Currently only **one** source code file can be covered but
  * it can be located in a whole project but need to be encapsulated and
  * a whole test suit is supported
* Currently polycover only supports **one** user-defined method call per line
* Currently there is a problem with umlauts so avoid them

## Installation

Just clone or download this repo.

## Usage

To see the list of arguments, run:
```
polycover --help
```

The arguments are:
```
.NET Framework source code coverage tool

Usage: polycover [options] [arguments]

Options:
        -v|--variant    the variant of coverage to be calculated: 'edge' or 'type'

Arguments:
        <CODEFILE>      Path to the source code file
        <SOLUTION>      Path to the solution
        <TESTPROJECT>   Path to the test project
        <TESTASSEMBLY>  Path to the test assembly
```

An example call could look like following:
```
polycover --variant "edge" "C:\MyApp\MyProject\MyCodeFile.cs" "C:\MyApp\MySolution.sln" "C:\MyApp\MyProjectTests\MyProjectTests.csproj" "C:\MyApp\MyProjectTests\bin\Debug\MyProjectTests.dll"
```

_Note: Mayby you have to add_ `<IsTestProject>true</IsTestProject>` _to your test.csproj file to get the tests run._

### Options

#### variant

The `--variant` option has to be either `edge` or `type`. It specifies what kind of coverage is getting calculated:
* `edge` coverage calculates the percentage of executed invocations of all possible target methods in the tested code triggered by tests.
* `type` coverage calculates the percentage of executed calls of all possible types on which each method can be called.

## Output

A file named the same as the source code file with "\_YoYoGraph.dgml" (for variant `edge`) -or- "\_InheritanceGraph.dgml" (for variant `type`) appended will be generated in the same directory.

## How It Works

polycover basically goes throgh the following pipeline:
1. Analyzes the given source code file using the [.NET Compiler Platform ("Roslyn")](https://github.com/dotnet/roslyn)
2. Either creates a "YoYo-Graph" from the analyzed code which is a call graph with consideration of polymorphism -or- creates a "inheritance graph" which displays the inheritance graph for each method seperatly
3. Modifies the source code file by inserting code to record either caller's line numbers -or- caller's type
4. Rebuilds the entire solution
5. Runs [coverlet](https://github.com/tonerdo/coverlet) to check which of the inserted statements were executed
6. Calculates the coverage result and marks covered nodes and links green, uncovered red
7. Restores the original source code file

## General Information

|                |            |
|----------------|------------|
| Author         | Felix Keck |
| .NET Framework | 4.6.1      |
| Visual Studio  | 2017       |
