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

        // getBaseClasses tests:
        // =====================
        [TestMethod()]
        public void getBaseClassesTest1()
        {
            var expected = new List<INamedTypeSymbol> { };
            var actual = testAnalysis.getBaseClasses(testAnalysis.getClassDeclSyntax("ExampleCode.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getBaseClassesTest2()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C1")
            };
            var actual = testAnalysis.getBaseClasses(testAnalysis.getClassDeclSyntax("ExampleCode.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getBaseClassesTest3()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C2"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C1")
            };
            var actual = testAnalysis.getBaseClasses(testAnalysis.getClassDeclSyntax("ExampleCode.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // getDerivedClasses tests:
        // ========================
        [TestMethod()]
        public void getDerivedClassesTest1()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C2"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C3_1"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C3_2")
            };
            var actual = testAnalysis.getDerivedClasses(testAnalysis.getClassDeclSyntax("ExampleCode.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getDerivedClassesTest2()
        {
            var expected = new List<INamedTypeSymbol>
            {
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C3_1"),
                testAnalysis.GetClassNamedTypeSymbol("ExampleCode.C3_2")
            };
            var actual = testAnalysis.getDerivedClasses(testAnalysis.getClassDeclSyntax("ExampleCode.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getDerivedClassesTest3()
        {
            var expected = new List<INamedTypeSymbol> { };
            var actual = testAnalysis.getDerivedClasses(testAnalysis.getClassDeclSyntax("ExampleCode.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // getMethods tests:
        // =================
        [TestMethod()]
        public void getMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.getMethodDeclSyntax("ExampleCode.C1.M1"),
                testAnalysis.getMethodDeclSyntax("ExampleCode.C1.M2"),
                testAnalysis.getMethodDeclSyntax("ExampleCode.C1.M3")
            };
            var actual = testAnalysis.getMethods(testAnalysis.getClassDeclSyntax("ExampleCode.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getMethodsTest2()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.getMethodDeclSyntax("ExampleCode.C2.M2"),
                testAnalysis.getMethodDeclSyntax("ExampleCode.C2.M4")
            };
            var actual = testAnalysis.getMethods(testAnalysis.getClassDeclSyntax("ExampleCode.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.getMethodDeclSyntax("ExampleCode.C3_1.M3"),
                testAnalysis.getMethodDeclSyntax("ExampleCode.C3_1.M4")
            };
            var actual = testAnalysis.getMethods(testAnalysis.getClassDeclSyntax("ExampleCode.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // getInvocations tests:
        // =====================
        [TestMethod()]
        public void getInvocationsTest1()
        {
            var expected = new List<IMethodSymbol>
            {
                testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C1.M2"),
                testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C1.M3")
            };
            var actual = testAnalysis.getInvocations(testAnalysis.getMethodDeclSyntax("ExampleCode.C1.M1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // toString() method should not be listed -> only user defined methods!
        [TestMethod()]
        public void getInvocationsTest2()
        {
            var expected = new List<IMethodSymbol>
            {
                testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C1.M3")
            };
            var actual = testAnalysis.getInvocations(testAnalysis.getMethodDeclSyntax("ExampleCode.C1.M2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getInvocationsTest3()
        {
            var expected = new List<IMethodSymbol> { };
            var actual = testAnalysis.getInvocations(testAnalysis.getMethodDeclSyntax("ExampleCode.C1.M3"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getInvocationsTest4()
        {
            var expected = new List<IMethodSymbol> { };
            var actual = testAnalysis.getInvocations(testAnalysis.getMethodDeclSyntax("ExampleCode.C2.M2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // Math.Pow should not be listed -> only user defined functions!
        [TestMethod()]
        public void getInvocationsTest5()
        {
            var expected = new List<IMethodSymbol> { };
            var actual = testAnalysis.getInvocations(testAnalysis.getMethodDeclSyntax("ExampleCode.C3_1.M4"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // getOverridingMethods tests:
        // ===========================
        [TestMethod()]
        public void getOverridingMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.getOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C1.M1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getOverridingMethodsTest2()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.getMethodDeclSyntax("ExampleCode.C2.M2")
            };
            var actual = testAnalysis.getOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C1.M2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getOverridingMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.getMethodDeclSyntax("ExampleCode.C3_1.M3")
            };
            var actual = testAnalysis.getOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C1.M3"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getOverridingMethodsTest4()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.getOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C2.M2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getOverridingMethodsTest5()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.getMethodDeclSyntax("ExampleCode.C3_1.M4"),
                testAnalysis.getMethodDeclSyntax("ExampleCode.C3_2.M4")
            };
            var actual = testAnalysis.getOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C2.M4"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void getOverridingMethodsTest6()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.getOverridingMethods(testAnalysis.GetMethodNamedTypeSymbol("ExampleCode.C3_2.M4"));
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
