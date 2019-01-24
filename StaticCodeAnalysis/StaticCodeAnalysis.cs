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
        public List<INamedTypeSymbol> GetBaseClasses(ClassDeclarationSyntax classDecl)
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
        public List<INamedTypeSymbol> GetDerivedClasses(ClassDeclarationSyntax classDecl)
        {
            List<INamedTypeSymbol> derivedClasses = new List<INamedTypeSymbol>();

            // init classes with the symbol of the given class
            List<INamedTypeSymbol> classes = new List<INamedTypeSymbol> { semMod.GetDeclaredSymbol(classDecl) };
            List<INamedTypeSymbol> subClasses = new List<INamedTypeSymbol>();

            while (classes.Count > 0)
            {
                // find all classes which derive directly from a member of the current list of derived classes
                subClasses = (from myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                              where classes.Contains(semMod.GetDeclaredSymbol(myClass).BaseType)
                              select semMod.GetDeclaredSymbol(myClass)).ToList();

                // add all found classes to return list
                derivedClasses.AddRange(subClasses);

                classes = subClasses;
            }

            return derivedClasses;
        }

        // returns a list of all methods declared in code
        public List<MethodDeclarationSyntax> GetAllMethods()
        {
            return root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        }

        // returns a list of all methods declared in a given class
        public List<MethodDeclarationSyntax> GetMethods(ClassDeclarationSyntax classDecl)
        {
            return classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        }

        // ToDo: should onyl return user defined methods, not .toString() or Math.Pow()!
        // returns a list of all methods called in a given method
        public List<IMethodSymbol> GetInvocations(MethodDeclarationSyntax methodDecl)
        {
            return methodDecl.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Select(invoc => (IMethodSymbol)semMod.GetSymbolInfo(invoc).Symbol).ToList();
        }

        // returns a list of all methods overriding a given method
        public List<MethodDeclarationSyntax> GetOverridingMethods(MethodDeclarationSyntax method)
        {
            List<MethodDeclarationSyntax> overridingMethods = new List<MethodDeclarationSyntax>();

            // init meths with given method
            List<MethodDeclarationSyntax> meths = new List<MethodDeclarationSyntax>() { method };
            List<MethodDeclarationSyntax> overrMeths = new List<MethodDeclarationSyntax>();

            while (meths.Count > 0)
            {
                // find all overriding methods of direct sub classes
                foreach (MethodDeclarationSyntax meth in meths)
                {
                    overrMeths.AddRange((from methodDecl in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                                         where semMod.GetDeclaredSymbol(methodDecl).OverriddenMethod == semMod.GetDeclaredSymbol(meth)
                                         select methodDecl).ToList());
                }

                // add all overriding methods to return list
                overridingMethods.AddRange(overrMeths);

                // set meths to a clone of overrMeths
                meths = new List<MethodDeclarationSyntax>(overrMeths);
                // reset overrMeths
                overrMeths.Clear();
            }

            return overridingMethods;
        }


        // helper functions:
        // =================

        // returns the class declaration syntax node for a given class name
        public ClassDeclarationSyntax GetClassDeclSyntax(string fullClassName)
        {
            return (from myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                    where semMod.GetDeclaredSymbol(myClass).OriginalDefinition.ToString() == fullClassName
                    select myClass).First();
        }

        // returns the method declaration syntax node for a given class name
        public MethodDeclarationSyntax GetMethodDeclSyntax(string fullMethodName)
        {
            return (from myMethod in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where (semMod.GetDeclaredSymbol(myMethod).ContainingType.OriginalDefinition.ToString()
                           + "." + semMod.GetDeclaredSymbol(myMethod).Name) == fullMethodName
                    select myMethod).First(); // assumes there is only one method with this specified name in the class -> need to be fixed!
        }

        // returns the full name of a given method declaration syntax
        public string GetFullMethodName(MethodDeclarationSyntax method)
        {
            return semMod.GetDeclaredSymbol(method).ContainingType.OriginalDefinition.ToString()
                   + "." + semMod.GetDeclaredSymbol(method).Name;
        }

        // returns the NamedTypeSymbol of a given class name
        public INamedTypeSymbol GetClassNamedTypeSymbol(string fullClassName)
        {
            return semMod.GetDeclaredSymbol(GetClassDeclSyntax(fullClassName));
        }

        // returns the NamedTypeSymbol of a given method name
        public IMethodSymbol GetMethodNamedTypeSymbol(string fullMethodName)
        {
            return semMod.GetDeclaredSymbol(GetMethodDeclSyntax(fullMethodName));
        }

        public YoYoGraph.Node GetCorrespondingNode(YoYoGraph graph, IMethodSymbol methSymb)
        {
            return (from node in graph.Nodes
                    where semMod.GetDeclaredSymbol(node.MethDecl) == methSymb
                    select node).First();
        }

        public YoYoGraph.Node GetCorrespondingNode(YoYoGraph graph, MethodDeclarationSyntax methDecl)
        {
            return (from node in graph.Nodes
                    where node.MethDecl == methDecl
                    select node).First();
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            //string path = @"..\..\..\ExampleCode\ExampleCode4.cs";
            //StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(path);

            //YoYoGraph testGraph = new YoYoGraph();
            //List<MethodDeclarationSyntax> methods = testAnalysis.GetAllMethods();

            //// create a node for each method
            //foreach (MethodDeclarationSyntax method in methods)
            //{
            //    YoYoGraph.Node newNode = new YoYoGraph.Node(method, testAnalysis.GetFullMethodName(method), testAnalysis.GetFullMethodName(method));
            //    testGraph.AddNode(newNode);
            //}

            //// add the links (invocations)
            //foreach (YoYoGraph.Node node in testGraph.Nodes)
            //{
            //    List<IMethodSymbol> invocs = testAnalysis.GetInvocations(node.MethDecl);
            //    foreach (IMethodSymbol invoc in invocs)
            //    {
            //        // direct call
            //        YoYoGraph.Link newLink = new YoYoGraph.Link(node.Id, testAnalysis.GetCorrespondingNode(testGraph, invoc).Id, "direct");
            //        testGraph.AddLink(newLink);

            //        // overriding method calls
            //        List<MethodDeclarationSyntax> overridingMethods = testAnalysis.GetOverridingMethods(invoc);
            //        foreach (MethodDeclarationSyntax overridingMethod in overridingMethods)
            //        {
            //            newLink = new YoYoGraph.Link(node.Id, testAnalysis.GetCorrespondingNode(testGraph, overridingMethod).Id, "overridden");
            //            testGraph.AddLink(newLink);
            //        }
            //    }
            //}

            //testGraph.Serialize(@"..\..\..\ExampleCode\ExampleCode4YoYoGraph.dgml");
        }
    }
}
