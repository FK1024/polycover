using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StaticCodeAnalysis
{
    public class StaticCodeAnalysis
    {
        private string code;
        private SyntaxTree tree;
        private SyntaxNode root;
        private CSharpCompilation compilation;
        private SemanticModel semMod;


        public StaticCodeAnalysis(string path)
        {
            code = File.ReadAllText(path);
            tree = CSharpSyntaxTree.ParseText(code);
            root = tree.GetRoot();
            compilation = CSharpCompilation.Create(null)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);
            semMod = compilation.GetSemanticModel(tree, false);
        }


        // returns a list of named type symbols of all classes which are direct or indirect base classes of a given class
        public List<INamedTypeSymbol> getBaseClasses(ClassDeclarationSyntax classDecl)
        {
            List<INamedTypeSymbol> baseClasses = new List<INamedTypeSymbol>();

            INamedTypeSymbol baseClass = semMod.GetDeclaredSymbol(classDecl).BaseType;

            while (baseClass.SpecialType != SpecialType.System_Object) // all classes derive from System.Object
            {
                baseClasses.Add(baseClass);
                baseClass = baseClass.BaseType;
            }

            return baseClasses;

            // works too:
            // classDecl.BaseList.Types.First().Type.ToString(); // problematic if class don't derives
        }

        // returns a list of named type symbols of all classes which are direct or indirect derived classes of a given class
        public List<INamedTypeSymbol> getDerivedClasses(ClassDeclarationSyntax classDecl)
        {
            List<INamedTypeSymbol> derivedClasses = new List<INamedTypeSymbol>();

            // init heirs with the symbol of the given class
            IEnumerable<INamedTypeSymbol> heirs = new List<INamedTypeSymbol> { semMod.GetDeclaredSymbol(classDecl) };
            IEnumerable<INamedTypeSymbol> nextHeirs = new List<INamedTypeSymbol>();

            while (heirs.Count() > 0)
            {
                // find all classes which derive directly from a member of the current list of derived classes
                nextHeirs = (from myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                             where heirs.Contains(semMod.GetDeclaredSymbol(myClass).BaseType)
                             select semMod.GetDeclaredSymbol(myClass)).ToList();

                // add all found classes to return list
                derivedClasses.AddRange(nextHeirs);

                heirs = nextHeirs;
            }

            return derivedClasses;
        }

        // returns a list of all methods declared in a given class
        public List<MethodDeclarationSyntax> getMethods(ClassDeclarationSyntax classDecl)
        {
            return classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        }

        // ToDo: should onyl return user defined methods, not .toString() or Math.Pow()!
        // returns a list of all methods called in a given method
        public List<IMethodSymbol> getInvocations(MethodDeclarationSyntax methodDecl)
        {
            return methodDecl.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Select(invoc => (IMethodSymbol)semMod.GetSymbolInfo(invoc).Symbol).ToList();
        }

        // returns a list of all methods overriding a given method
        public List<MethodDeclarationSyntax> getOverridingMethods(IMethodSymbol method)
        {
            return (from methodDecl in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where semMod.GetDeclaredSymbol(methodDecl).OverriddenMethod == method
                    select methodDecl).ToList();
        }


        // helper functions:
        // =================

        // returns the class declaration syntax node for a given class name
        public ClassDeclarationSyntax getClassDeclSyntax(string fullClassName)
        {
            return (from myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                    where semMod.GetDeclaredSymbol(myClass).OriginalDefinition.ToString() == fullClassName
                    select myClass).First();
        }

        // returns the class declaration syntax node for a given class name
        public MethodDeclarationSyntax getMethodDeclSyntax(string fullMethodName)
        {
            return (from myMethod in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where (semMod.GetDeclaredSymbol(myMethod).ContainingType.OriginalDefinition.ToString()
                           + "." + semMod.GetDeclaredSymbol(myMethod).Name) == fullMethodName
                    select myMethod).First(); // assumes there is only one method with this specified name in the class -> need to be fixed!
        }

        // returns the NamedTypeSymbol of a given class name
        public INamedTypeSymbol GetClassNamedTypeSymbol(string fullClassName)
        {
            return semMod.GetDeclaredSymbol(getClassDeclSyntax(fullClassName));
        }

        // returns the NamedTypeSymbol of a given method name
        public IMethodSymbol GetMethodNamedTypeSymbol(string fullMethodName)
        {
            return semMod.GetDeclaredSymbol(getMethodDeclSyntax(fullMethodName));
        }


        static void Main(string[] args)
        {
            string path = @"C:\Users\fkeck\OneDrive\Studium\Semester07\Thesis\StaticCodeAnalysis\ExampleCode\ExampleCode.cs";
            StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(path);


        }
    }
}
