using System;
using System.Collections.Generic;
using System.Drawing;
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


        // returns a list of all classes defined in code
        public List<ClassDeclarationSyntax> GetAllClasses()
        {
            return root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        }

        // returns a list of named type symbols of all classes which are direct or indirect base classes of a given class
        public List<ClassDeclarationSyntax> GetBaseClasses(ClassDeclarationSyntax classDecl)
        {
            List<INamedTypeSymbol> baseClasses = new List<INamedTypeSymbol>();

            INamedTypeSymbol baseClass = semMod.GetDeclaredSymbol(classDecl).BaseType;

            while (baseClass.SpecialType != SpecialType.System_Object) // all classes derive from System.Object
            {
                baseClasses.Add(baseClass);
                baseClass = baseClass.BaseType;
            }

            List<ClassDeclarationSyntax> baseClassesDeclarations = baseClasses.Select(myBaseClass => GetClassDeclSyntax(myBaseClass)).ToList();
            baseClassesDeclarations.RemoveAll(item => item == null);

            return baseClassesDeclarations;

            // works too:
            // classDecl.BaseList.Types.First().Type.ToString(); // problematic if class don't derives
        }

        // returns a list of named type symbols of all classes which are direct or indirect derived classes of a given class
        public List<ClassDeclarationSyntax> GetDerivedClasses(ClassDeclarationSyntax classDecl)
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

            List<ClassDeclarationSyntax> derivedClassesDeclarations = derivedClasses.Select(myDerivedClass => GetClassDeclSyntax(myDerivedClass)).ToList();
            derivedClassesDeclarations.RemoveAll(item => item == null);

            return derivedClassesDeclarations;
        }

        // returns a list of all methods declared in a given class
        public List<MethodDeclarationSyntax> GetMethods(ClassDeclarationSyntax classDecl)
        {
            return classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        }

        //ToDo: if invoced method is defined in an interface, return the corresponding method of the class which implements this interface
        // returns a list of all methods called in a given method
        public List<MethodDeclarationSyntax> GetInvocations(MethodDeclarationSyntax methodDecl)
        {
            List<IMethodSymbol> invocationSymbols = methodDecl.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Select(invoc => (IMethodSymbol)semMod.GetSymbolInfo(invoc).Symbol).ToList();

            List<MethodDeclarationSyntax> invocationDeclarations = invocationSymbols.Select(invoc => GetMethodDeclSyntax(invoc)).ToList();
            invocationDeclarations.RemoveAll(item => item == null);
            
            return invocationDeclarations;
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

        // returns the corresponding ClassDeclarationSyntax of a given NamedTypeSymbol
        public ClassDeclarationSyntax GetClassDeclSyntax(INamedTypeSymbol classSymb)
        {
            return (from myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                    where semMod.GetDeclaredSymbol(myClass).Equals(classSymb)
                    select myClass).FirstOrDefault();
        }

        // returns the class declaration syntax node for a given class name
        public ClassDeclarationSyntax GetClassDeclSyntax(string fullClassName)
        {
            return (from myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                    where semMod.GetDeclaredSymbol(myClass).OriginalDefinition.ToString() == fullClassName
                    select myClass).First();
        }

        // returns the full name of a given class declaration syntax
        public string GetFullClassName(ClassDeclarationSyntax classDecl)
        {
            return semMod.GetDeclaredSymbol(classDecl).OriginalDefinition.ToString();
        }


        // returns the corresponding ClassDeclarationSyntax of a given MethodSymbol
        public MethodDeclarationSyntax GetMethodDeclSyntax(IMethodSymbol methSymb)
        {
            return (from myMethod in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where semMod.GetDeclaredSymbol(myMethod).Equals(methSymb)
                    select myMethod).FirstOrDefault(); // ...OrDefault returns null for non user defined methods such as .toString()
        }

        // returns the method declaration syntax node for a given class name
        public MethodDeclarationSyntax GetMethodDeclSyntax(string fullMethodName)
        {
            return (from myMethod in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where (semMod.GetDeclaredSymbol(myMethod).ContainingType.OriginalDefinition.ToString()
                           + "." + semMod.GetDeclaredSymbol(myMethod).Name) == fullMethodName
                    select myMethod).First(); // assumes there is only one method with this specified name in the class -> only to get the classDecls for checking test results
        }
        
        // returns the full name of a given method declaration syntax
        public string GetFullMethodName(MethodDeclarationSyntax methodDecl)
        {
            return semMod.GetDeclaredSymbol(methodDecl).ContainingType.OriginalDefinition.ToString()
                   + "." + semMod.GetDeclaredSymbol(methodDecl).Name;
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
            string codePath = @"..\..\..\ExampleCode\ExampleCode.cs";
            StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(codePath);

            YoYoGraph testGraph = new YoYoGraph();
            List<ClassDeclarationSyntax> classes = testAnalysis.GetAllClasses();

            // Get all the values from the KnownColor enumeration.
            Array colorsArray = Enum.GetValues(typeof(KnownColor));
            KnownColor[] allColors = new KnownColor[colorsArray.Length];
            Array.Copy(colorsArray, allColors, colorsArray.Length);
            
            int colorCounter = 36; // good index to start at to have at least 34 easily distinguishable colors
            foreach (ClassDeclarationSyntax myClass in classes)
            {
                // select a color for each class
                string classColor = ColorTranslator.ToHtml(Color.FromArgb(Color.FromKnownColor(allColors[colorCounter]).ToArgb()));
                
                // create a category for each class
                string categoryName = testAnalysis.GetFullClassName(myClass);
                testGraph.AddCategory(new YoYoGraph.Category(categoryName, classColor));

                // create a node for each method
                List<MethodDeclarationSyntax> methods = testAnalysis.GetMethods(myClass);
                foreach (MethodDeclarationSyntax method in methods)
                {
                    string nodeName = testAnalysis.GetFullMethodName(method);
                    testGraph.AddNode(new YoYoGraph.Node(method, nodeName, nodeName, categoryName));
                }

                colorCounter++;
            }

            // add the links (invocations)
            foreach (YoYoGraph.Node node in testGraph.Nodes)
            {
                List<MethodDeclarationSyntax> invocs = testAnalysis.GetInvocations(node.MethDecl);
                foreach (MethodDeclarationSyntax invoc in invocs)
                {
                    // direct call
                    testGraph.AddLink(new YoYoGraph.Link(node.Id, testAnalysis.GetCorrespondingNode(testGraph, invoc).Id));

                    // overriding method calls
                    List<MethodDeclarationSyntax> overridingMethods = testAnalysis.GetOverridingMethods(invoc);
                    foreach (MethodDeclarationSyntax overridingMethod in overridingMethods)
                    {
                        testGraph.AddLink(new YoYoGraph.Link(node.Id, testAnalysis.GetCorrespondingNode(testGraph, overridingMethod).Id));
                    }
                }
            }

            string DGMLPath = codePath.Substring(0, codePath.Length - 3) + "YoYoGraph.dgml";
            testGraph.Serialize(DGMLPath);
        }
    }
}
