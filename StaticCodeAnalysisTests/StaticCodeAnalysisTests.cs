using Microsoft.VisualStudio.TestTools.UnitTesting;
using StaticCodeAnalysis;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace StaticCodeAnalysis.Tests
{
    [TestClass()]
    public class StaticCodeAnalysisTests
    {
        static string path = @"..\..\..\ExampleCode\ExampleCode.cs";
        StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(path);


        public bool NestedListsAreEqual<T>(List<List<T>> nestedList1, List<List<T>> nestedList2)
        {
            if (nestedList1.Count != nestedList2.Count) return false;
            for (int i = 0; i < nestedList1.Count; i++)
            {
                if (!nestedList1[i].SequenceEqual(nestedList2[i])) return false;
            }
            return true;
        }

        // GetBaseClasses tests:
        // =====================
        [TestMethod()]
        public void GetBaseClassesTest1()
        {
            var expected = new List<ClassDeclarationSyntax> { };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest2()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode.C1")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest3()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode.C2"),
                testAnalysis.GetClassDeclSyntax("ExampleCode.C1")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetDerivedClasses tests:
        // ========================
        [TestMethod()]
        public void GetDerivedClassesTest1()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode.C2"),
                testAnalysis.GetClassDeclSyntax("ExampleCode.C3_1"),
                testAnalysis.GetClassDeclSyntax("ExampleCode.C3_2")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest2()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode.C3_1"),
                testAnalysis.GetClassDeclSyntax("ExampleCode.C3_2")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C2"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest3()
        {
            var expected = new List<ClassDeclarationSyntax> { };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode.C3_1"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetAllMethods test:
        // ===================
        [TestMethod()]
        public void GetAllMethodsTest()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M1"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M2"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M3"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M2"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M3"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M4"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M3"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M4"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_2.M4"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_2.M5")
            };
            var actual = testAnalysis.GetAllMethods();
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetInvocations tests:
        // =====================
        [TestMethod()]
        public void GetInvocationsTest1()
        {
            var expected = new List<List<MethodDeclarationSyntax>>
            {
                new List<MethodDeclarationSyntax>
                {
                    testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M2"),
                    testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M2")
                },
                new List<MethodDeclarationSyntax>
                {
                    testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M3"),
                    testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M3"),
                    testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M3")
                }
            };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M1"));
            Assert.IsTrue(NestedListsAreEqual(expected, actual));
        }

        [TestMethod()]
        public void GetInvocationsTest2()
        {
            var expected = new List<List<MethodDeclarationSyntax>>
            {
                new List<MethodDeclarationSyntax>
                {
                    testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M3"),
                    testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M3"),
                    testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M3")
                }
            };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M2"));
            Assert.IsTrue(NestedListsAreEqual(expected, actual));
        }

        [TestMethod()]
        public void GetInvocationsTest3()
        {
            var expected = new List<List<MethodDeclarationSyntax>> { };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C1.M3"));
            Assert.IsTrue(NestedListsAreEqual(expected, actual));
        }

        [TestMethod()]
        public void GetInvocationsTest4()
        {
            var expected = new List<List<MethodDeclarationSyntax>> { };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C2.M2"));
            Assert.IsTrue(NestedListsAreEqual(expected, actual));
        }

        [TestMethod()]
        public void GetInvocationsTest5()
        {
            var expected = new List<List<MethodDeclarationSyntax>> { };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode.C3_1.M4"));
            Assert.IsTrue(NestedListsAreEqual(expected, actual));
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
