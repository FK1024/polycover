using Microsoft.VisualStudio.TestTools.UnitTesting;
using polycover;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace polycover.Tests
{
    [TestClass()]
    public class StaticCodeAnalysisTests1
    {
        static string path = @"..\..\..\ExampleCode\ExampleCode1.cs";
        StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(path);


        // GetBaseClasses tests:
        // =====================
        [TestMethod()]
        public void GetBaseClassesTest1()
        {
            var expected = new List<ClassDeclarationSyntax> { };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode1.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest2()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C1")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode1.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest3()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C2"),
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C1")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode1.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetDirectDerivedClasses tests:
        // ==============================
        [TestMethod]
        public void GetDirectDerivedClassesTest1()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C2")
            };
            var actual = testAnalysis.GetDirectDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode1.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDirectDerivedClassesTest2()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C3_1"),
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C3_2")
            };
            var actual = testAnalysis.GetDirectDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode1.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDirectDerivedClassesTest3()
        {
            var expected = new List<ClassDeclarationSyntax> { };
            var actual = testAnalysis.GetDirectDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode1.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetDerivedClasses tests:
        // ========================
        [TestMethod()]
        public void GetDerivedClassesTest1()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C2"),
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C3_1"),
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C3_2")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode1.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest2()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C3_1"),
                testAnalysis.GetClassDeclSyntax("ExampleCode1.C3_2")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode1.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest3()
        {
            var expected = new List<ClassDeclarationSyntax> { };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode1.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetAllMethods test:
        // ===================
        [TestMethod()]
        public void GetAllMethodsTest()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M1(int)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M2(int, int)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M3(int, bool, string)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M2(int, int)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M3(int, bool, string)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M4(int, int)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_1.M3(int, bool, string)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_1.M4(int, int)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_2.M4(int, int)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_2.M5(int)")
            };
            var actual = testAnalysis.GetAllMethods();
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetInvocations tests:
        // =====================
        [TestMethod()]
        public void GetInvocationsTest1()
        {
            var expected = new List<StaticCodeAnalysis.Invocation>
            {
                new StaticCodeAnalysis.Invocation(
                    new List<int> { 29 },
                    new List<MethodDeclarationSyntax>
                    {
                        testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M2(int, int)"),
                        testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M2(int, int)")
                    }),
                new StaticCodeAnalysis.Invocation(
                    new List<int> { 33 },
                    new List<MethodDeclarationSyntax>
                    {
                        testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M3(int, bool, string)"),
                        testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M3(int, bool, string)"),
                        testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_1.M3(int, bool, string)")
                    })
            };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M1(int)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetInvocationsTest2()
        {
            var expected = new List<StaticCodeAnalysis.Invocation>
            {
                new StaticCodeAnalysis.Invocation(
                    new List<int> { 39 },
                    new List<MethodDeclarationSyntax>
                    {
                        testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M3(int, bool, string)"),
                        testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M3(int, bool, string)"),
                        testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_1.M3(int, bool, string)")
                    })
            };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M2(int, int)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetInvocationsTest3()
        {
            var expected = new List<StaticCodeAnalysis.Invocation> { };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M3(int, bool, string)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetInvocationsTest4()
        {
            var expected = new List<StaticCodeAnalysis.Invocation> { };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M2(int, int)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetInvocationsTest5()
        {
            var expected = new List<StaticCodeAnalysis.Invocation> { };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_1.M4(int, int)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetOverridingMethods tests:
        // ===========================
        [TestMethod()]
        public void GetOverridingMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M1(int)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest2()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M2(int, int)")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M2(int, int)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M3(int, bool, string)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_1.M3(int, bool, string)")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C1.M3(int, bool, string)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest4()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_1.M3(int, bool, string)")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M3(int, bool, string)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest5()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_1.M4(int, int)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_2.M4(int, int)")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C2.M4(int, int)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest6()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("int ExampleCode1.C3_2.M4(int, int)"));
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
