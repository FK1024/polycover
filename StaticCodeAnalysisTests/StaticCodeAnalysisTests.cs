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
    public class StaticCodeAnalysisTests
    {
        static string path = @"..\..\..\ExampleCode\ExampleCode.cs";
        StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(path);

        // GetBaseClasses tests:
        // =====================
        [TestMethod()]
        public void GetBaseClassesTest1()
        {
            var expected = new List<INamedTypeSymbol> { };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest2()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C1")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest3()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C2"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C1")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetDerivedClasses tests:
        // ========================
        [TestMethod()]
        public void GetDerivedClassesTest1()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C2"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C3_1"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C3_2")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest2()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C3_1"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C3_2")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest3()
        {
            var expected = new List<INamedTypeSymbol> { };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetMethods tests:
        // =================
        [TestMethod()]
        public void GetMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M1"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M2"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M3")
            };
            var actual = testAnalysis.GetMethods(testAnalysis.GetClassDeclSyntax("ExampleCode.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetMethodsTest2()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M2"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M3"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M4")
            };
            var actual = testAnalysis.GetMethods(testAnalysis.GetClassDeclSyntax("ExampleCode.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M3"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M4")
            };
            var actual = testAnalysis.GetMethods(testAnalysis.GetClassDeclSyntax("ExampleCode.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetInvocations tests:
        // =====================
        [TestMethod()]
        public void GetInvocationsTest1()
        {
            var expected = new List<IMethodSymbol>
            {
                testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C1.M2"),
                testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C1.M3")
            };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // toString() method should not be listed -> only user defined methods!
        [TestMethod()]
        public void GetInvocationsTest2()
        {
            var expected = new List<IMethodSymbol>
            {
                testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C1.M3")
            };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetInvocationsTest3()
        {
            var expected = new List<IMethodSymbol> { };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M3"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetInvocationsTest4()
        {
            var expected = new List<IMethodSymbol> { };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // Math.Pow should not be listed -> only user defined functions!
        [TestMethod()]
        public void GetInvocationsTest5()
        {
            var expected = new List<IMethodSymbol> { };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M4"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetOverridingMethods tests:
        // ===========================
        [TestMethod()]
        public void GetOverridingMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest2()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M2")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M3"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M3")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M3"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest4()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M3")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M3"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest5()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M4"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_2.M4")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M4"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest6()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_2.M4"));
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
