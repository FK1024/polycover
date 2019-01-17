using Microsoft.VisualStudio.TestTools.UnitTesting;
using StaticCodeAnalysis;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StaticCodeAnalysis.Tests
{
    [TestClass()]
    public class StaticCodeAnalysisTests2
    {
        static string path = @"..\..\..\ExampleCode\ExampleCode2.cs";
        StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(path);

        // GetBaseClasses tests:
        // =====================
        [TestMethod()]
        public void GetBaseClassesTest1()
        {
            var expected = new List<INamedTypeSymbol> { };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest2()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.BaseClass")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest3()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.DerivedAbstractClass"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.BaseClass")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetDerivedClasses tests:
        // ========================
        [TestMethod()]
        public void GetDerivedClassesTest1()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.DerivedAbstractClass"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.DerivedConcreteClass")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest2()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.DerivedConcreteClass")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest3()
        {
            var expected = new List<INamedTypeSymbol> { };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetMethods tests:
        // =================
        [TestMethod()]
        public void GetMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.BaseClass.Method1")
            };
            var actual = testAnalysis.GetMethods(testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetMethodsTest2()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedAbstractClass.Method1"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedAbstractClass.Method2")
            };
            var actual = testAnalysis.GetMethods(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedConcreteClass.Method2"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedConcreteClass.Method3")
            };
            var actual = testAnalysis.GetMethods(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass"));
            CollectionAssert.AreEqual(expected, actual);
        }
        
        // GetOverridingMethods tests:
        // ===========================
        [TestMethod()]
        public void GetOverridingMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedAbstractClass.Method1")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode2.BaseClass.Method1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest2()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode2.DerivedAbstractClass.Method1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedConcreteClass.Method2")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode2.DerivedAbstractClass.Method2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest4()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode2.DerivedConcreteClass.Method2"));
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
