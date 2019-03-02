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

        // returns all methods declared in a class
        public List<MethodDeclarationSyntax> GetAllMethods()
        {
            return (from method in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where method.Parent is ClassDeclarationSyntax
                    select method).ToList();
        }

        // for a given method returns a list of all invocations which are lists of all possible called methods (due to interfaces and overriding)
        public List<List<MethodDeclarationSyntax>> GetInvocations(MethodDeclarationSyntax methodDecl)
        {
            List<MethodDeclarationSyntax> directInvocations = methodDecl.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Select(invoc => GetMethodDeclSyntax(semMod.GetSymbolInfo(invoc).Symbol as IMethodSymbol))
                .Distinct().ToList();

            directInvocations.RemoveAll(item => item == null); // remove non user defined methods which are null

            List<List<MethodDeclarationSyntax>> allPossibleInvocations = new List<List<MethodDeclarationSyntax>>();

            foreach (MethodDeclarationSyntax invoc in directInvocations)
            {
                if (invoc.Parent is InterfaceDeclarationSyntax)
                {
                    // if method is declared in an interface add the list of all implementing methods instead + each overrides
                    List<MethodDeclarationSyntax> possibleInvocations = new List<MethodDeclarationSyntax>();
                    List<MethodDeclarationSyntax> implemMeths = GetInterfaceMethodImplementingMethods(invoc);
                    foreach (MethodDeclarationSyntax implemMeth in implemMeths)
                    {
                        List<MethodDeclarationSyntax> overrMethsAndSelf = GetOverridingMethods(implemMeth);
                        overrMethsAndSelf.Insert(0, implemMeth);
                        possibleInvocations.AddRange(overrMethsAndSelf);
                    }
                    allPossibleInvocations.Add(possibleInvocations);
                }
                else
                {
                    // add the method itself and the overrides
                    List<MethodDeclarationSyntax> overrMethsAndSelf = GetOverridingMethods(invoc);
                    overrMethsAndSelf.Insert(0, invoc);

                    allPossibleInvocations.Add(overrMethsAndSelf);
                }
            }

            return allPossibleInvocations;
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


        // returns all methods which implements a given method declared in an interface
        public List<MethodDeclarationSyntax> GetInterfaceMethodImplementingMethods(MethodDeclarationSyntax methodDecl)
        {
            List<MethodDeclarationSyntax> implementingMethods = new List<MethodDeclarationSyntax>();
            foreach (ClassDeclarationSyntax myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
            {
                MethodDeclarationSyntax implementingMethod = GetMethodDeclSyntax(semMod.GetDeclaredSymbol(myClass)
                    .FindImplementationForInterfaceMember(semMod.GetDeclaredSymbol(methodDecl)) as IMethodSymbol);
                if (!implementingMethods.Contains(implementingMethod))
                {
                    implementingMethods.Add(implementingMethod);
                }
            }
            return implementingMethods;
        }


        public YoYoGraph.Node GetCorrespondingNode(YoYoGraph graph, MethodDeclarationSyntax methDecl)
        {
            return (from node in graph.Nodes
                    where node.MethDecl == methDecl
                    select node).First();
        }
    }
}
