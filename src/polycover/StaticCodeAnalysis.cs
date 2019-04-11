using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace polycover
{
    public class StaticCodeAnalysis
    {
        private SyntaxNode root;
        private SemanticModel semMod;


        public StaticCodeAnalysis(string file)
        {
            string code = File.ReadAllText(file);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            root = tree.GetRoot();
            CSharpCompilation compilation = CSharpCompilation.Create(null)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);
            semMod = compilation.GetSemanticModel(tree, false);
        }


        // returns the root of the syntax tree
        public SyntaxNode GetRoot()
        {
            return root;
        }

        // returns a list of all classes which are direct or indirect base classes of a given class
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
        }

        // returns a list of all classes which are direct derived classes of a given class
        public List<ClassDeclarationSyntax> GetDirectDerivedClasses(ClassDeclarationSyntax classDecl)
        {
            return (from myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                    where semMod.GetDeclaredSymbol(myClass).BaseType == semMod.GetDeclaredSymbol(classDecl)
                    select myClass).ToList();
        }

        // returns a list of all classes which are direct or indirect derived classes of a given class
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

        // returns whether the given class is abstract
        public bool IsClassAbstract(ClassDeclarationSyntax classDecl)
        {
            return semMod.GetDeclaredSymbol(classDecl).IsAbstract;
        }

        // checks whether a given class has a method which overrides or hides a given method
        public bool ClassOverridesOrHidesMethod(ClassDeclarationSyntax classDecl, MethodDeclarationSyntax method)
        {
            IMethodSymbol methodSymb = semMod.GetDeclaredSymbol(method);
            List<MethodDeclarationSyntax> methods = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            return (from meth in methods
                    let methSymb = semMod.GetDeclaredSymbol(meth)
                    where methSymb.Name == methodSymb.Name
                    && methSymb.ReturnType == methodSymb.ReturnType
                    && methSymb.Parameters.Select(p => p.Type).SequenceEqual(methodSymb.Parameters.Select(p => p.Type))
                    select meth).Any();
        }

        public struct InheritanceTree
        {
            public MethodDeclarationSyntax method;
            public InheritanceNode rootClass;

            public InheritanceTree(MethodDeclarationSyntax method, InheritanceNode rootClass)
            {
                this.method = method;
                this.rootClass = rootClass;
            }

            public override bool Equals(object obj)
            {
                InheritanceTree? testObj = obj as InheritanceTree?;

                if (testObj.Value.method == null)
                {
                    return false;
                }

                return testObj.Value.method == this.method
                    && testObj.Value.rootClass.Equals(this.rootClass);
            }

            public override int GetHashCode()
            {
                return (this.method, this.rootClass).GetHashCode();
            }
        }

        // returns the InheritanceTree for a given method
        public InheritanceTree GetInheritanceTree(MethodDeclarationSyntax method)
        {
            InheritanceNode rootClass = new InheritanceNode(GetClass(method));

            InheritanceNode tree = CreateSubTree(rootClass, method);

            return new InheritanceTree(method, rootClass);
        }

        // returns all methods declared in a class
        public List<MethodDeclarationSyntax> GetAllMethods()
        {
            return (from method in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where method.Parent is ClassDeclarationSyntax
                    select method).ToList();
        }

        // returns all non-static methods declared in a class
        public List<MethodDeclarationSyntax> GetAllNonStaticMethods()
        {
            return (from method in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where method.Parent is ClassDeclarationSyntax
                    && !semMod.GetDeclaredSymbol(method).IsStatic
                    select method).ToList();
        }

        // returns the class in which a given method is contained in
        public ClassDeclarationSyntax GetClass(MethodDeclarationSyntax method)
        {
            return method.Ancestors().OfType<ClassDeclarationSyntax>().First();
        }

        // returns the line of the first statement of a given method
        public int GetMethodBodyStartLine(MethodDeclarationSyntax method)
        {
            if (method.Body.Statements.Any())
            {
                return method.Body.Statements.First().GetLocation().GetLineSpan().StartLinePosition.Line;
            }
            else
            {
                int openBraceLine = method.Body.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
                if (openBraceLine == method.Body.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line)
                {
                    return openBraceLine;
                }
                else
                {
                    return openBraceLine + 1;
                }
            }
        }
        
        public struct Invocation
        {
            public List<int> Lines; // the line numbers where the invocation is written
            public List<MethodDeclarationSyntax> Methods; // the callee options (due to interfaces and overriding)

            public Invocation(List<int> lines, List<MethodDeclarationSyntax> methods)
            {
                this.Lines = lines;
                this.Methods = methods;
            }

            public override bool Equals(object obj)
            {
                Invocation? testObj = obj as Invocation?;

                if (testObj.Value.Methods == null)
                {
                    return false;
                }

                return testObj.Value.Lines.SequenceEqual(this.Lines)
                    && testObj.Value.Methods.SequenceEqual(this.Methods);
            }

            public override int GetHashCode()
            {
                return (this.Lines, this.Methods).GetHashCode();
            }
        }

        // returns a list of all Invocations in a given method
        public List<Invocation> GetInvocations(MethodDeclarationSyntax methodDecl)
        {
            // output list
            List<Invocation> allPossibleInvocations = new List<Invocation>();

            // get all invocation expressions
            List<InvocationExpressionSyntax> invocationExpressions = methodDecl.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

            if (!invocationExpressions.Any()) return allPossibleInvocations;

            // transform each invocation to the struct Invocation(callee, line number)
            List<Invocation> directInvocations = invocationExpressions.Select(invocExpr => new Invocation(
                new List<int> { invocExpr.GetLocation().GetLineSpan().StartLinePosition.Line }, // the line number of the invocation, NOTE: the line numbers start at 0!
                new List<MethodDeclarationSyntax> { GetMethodDeclSyntax(semMod.GetSymbolInfo(invocExpr).Symbol as IMethodSymbol) })) // the invoced method
                .ToList();

            // remove non user defined callee's which are null
            directInvocations.RemoveAll(invoc => invoc.Methods.FirstOrDefault() == null);

            if (!directInvocations.Any()) return allPossibleInvocations;

            // remove duplicate invocations but add the line numbers to the unique one
            directInvocations = (from invoc in directInvocations
                                group invoc.Lines by invoc.Methods.First() into grouped
                                select new Invocation(grouped.SelectMany(l => l).ToList(), new List<MethodDeclarationSyntax> { grouped.Key })).ToList();
            
            foreach (Invocation invoc in directInvocations)
            {
                if (invoc.Methods.First().Parent is InterfaceDeclarationSyntax)
                {
                    // if method is declared in an interface add the list of all implementing methods instead + each overrides
                    List<MethodDeclarationSyntax> possibleInvocations = new List<MethodDeclarationSyntax>();
                    List<MethodDeclarationSyntax> implemMeths = GetInterfaceMethodImplementingMethods(invoc.Methods.First());
                    foreach (MethodDeclarationSyntax implemMeth in implemMeths)
                    {
                        possibleInvocations.AddRange(GetOverridingMethodsAndSelf(implemMeth));
                    }
                    allPossibleInvocations.Add(new Invocation(invoc.Lines, possibleInvocations));
                }
                else
                {
                    // add the method itself and the overrides
                    allPossibleInvocations.Add(new Invocation(invoc.Lines, GetOverridingMethodsAndSelf(invoc.Methods.First())));
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

        // returns a list of all methods overriding a given method and the method itself
        public List<MethodDeclarationSyntax> GetOverridingMethodsAndSelf(MethodDeclarationSyntax method)
        {
            List<MethodDeclarationSyntax> result = GetOverridingMethods(method);
            result.Insert(0, method);
            return result;
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
        public ClassDeclarationSyntax GetClassDeclSyntax(string classId)
        {
            return (from myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                    where GetClassId(myClass) == classId
                    select myClass).First();
        }

        // returns the full name of a given class declaration syntax
        public string GetClassId(ClassDeclarationSyntax classDecl)
        {
            return semMod.GetDeclaredSymbol(classDecl).ToString();
        }


        // returns the corresponding ClassDeclarationSyntax of a given MethodSymbol
        public MethodDeclarationSyntax GetMethodDeclSyntax(IMethodSymbol methSymb)
        {
            return (from myMethod in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where semMod.GetDeclaredSymbol(myMethod).Equals(methSymb)
                    select myMethod).FirstOrDefault(); // ...OrDefault returns null for non user defined methods such as .toString()
        }

        // returns the method declaration syntax node for a given class name
        public MethodDeclarationSyntax GetMethodDeclSyntax(string methodId)
        {
            return (from myMethod in root.DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>()
                    where GetMethodId(myMethod) == methodId
                    select myMethod).First();
        }

        // returns the full name of a given method declaration syntax
        public string GetMethodId(MethodDeclarationSyntax methodDecl)
        {
            return semMod.GetDeclaredSymbol(methodDecl).ReturnType.ToString() + " " + semMod.GetDeclaredSymbol(methodDecl).ToString();
        }
        

        // returns all methods which implements a given method declared in an interface
        public List<MethodDeclarationSyntax> GetInterfaceMethodImplementingMethods(MethodDeclarationSyntax methodDecl)
        {
            List<MethodDeclarationSyntax> implementingMethods = new List<MethodDeclarationSyntax>();
            foreach (ClassDeclarationSyntax myClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
            {
                MethodDeclarationSyntax implementingMethod = GetMethodDeclSyntax(semMod.GetDeclaredSymbol(myClass)
                    .FindImplementationForInterfaceMember(semMod.GetDeclaredSymbol(methodDecl)) as IMethodSymbol);
                if (implementingMethod != null && !implementingMethods.Contains(implementingMethod))
                {
                    implementingMethods.Add(implementingMethod);
                }
            }
            return implementingMethods;
        }

        // recoursivly creates an inheritance tree for a given method and it's given base class
        public InheritanceNode CreateSubTree(InheritanceNode tree, MethodDeclarationSyntax method)
        {
            ClassDeclarationSyntax baseClass = tree.GetBaseClass();
            List<ClassDeclarationSyntax> methodInheritedClasses = GetDirectDerivedClasses(baseClass).Where(c => !ClassOverridesOrHidesMethod(c, method)).ToList();
            tree.SetSubClasses(methodInheritedClasses.Select(c => new InheritanceNode(c)).ToList());

            foreach (var subClass in tree.GetSubClasses())
            {
                CreateSubTree(subClass, method);
            }

            return tree;
        }
    }
}
