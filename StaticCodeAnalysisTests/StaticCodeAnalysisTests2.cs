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

        // getBaseClasses tests:
        // =====================
        [TestMethod()]
        public void getBaseClassesTest1()
        {
            var expected = new List<INamedTypeSymbol> { };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getBaseClassesTest2()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.BaseClass")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getBaseClassesTest3()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.DerivedAbstractClass"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.BaseClass")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // getDerivedClasses tests:
        // ========================
        [TestMethod()]
        public void getDerivedClassesTest1()
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
        public void getDerivedClassesTest2()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode2.DerivedConcreteClass")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getDerivedClassesTest3()
        {
            var expected = new List<INamedTypeSymbol> { };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // getMethods tests:
        // =================
        [TestMethod()]
        public void getMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.BaseClass.Method1")
            };
            var actual = testAnalysis.GetMethods(testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getMethodsTest2()
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
        public void getMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedConcreteClass.Method2"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedConcreteClass.Method3")
            };
            var actual = testAnalysis.GetMethods(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass"));
            CollectionAssert.AreEqual(expected, actual);
        }
        
        // getOverridingMethods tests:
        // ===========================
        [TestMethod()]
        public void getOverridingMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedAbstractClass.Method1")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode2.BaseClass.Method1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getOverridingMethodsTest2()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode2.DerivedAbstractClass.Method1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getOverridingMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode2.DerivedConcreteClass.Method2")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode2.DerivedAbstractClass.Method2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getOverridingMethodsTest4()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode2.DerivedConcreteClass.Method2"));
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
