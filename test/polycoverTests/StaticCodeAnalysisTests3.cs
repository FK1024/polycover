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
    public class StaticCodeAnalysisTests3
    {
        static string path = @"..\..\..\ExampleCode\ExampleCode3.cs";
        StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(path);


        // ClassOverridesOrHidesMethod tests:
        // ==================================
        [TestMethod]
        public void ClassOverridesOrHidesMethodTest1()
        {
            var expected = false;
            var actual = testAnalysis.ClassOverridesOrHidesMethod(testAnalysis.GetClassDeclSyntax("ExampleCode3.B"), testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.M"));
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void ClassOverridesOrHidesMethodTest2()
        {
            var expected = true;
            var actual = testAnalysis.ClassOverridesOrHidesMethod(testAnalysis.GetClassDeclSyntax("ExampleCode3.B"), testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.N"));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ClassOverridesOrHidesMethodTest3()
        {
            var expected = true;
            var actual = testAnalysis.ClassOverridesOrHidesMethod(testAnalysis.GetClassDeclSyntax("ExampleCode3.B"), testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.O"));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ClassOverridesOrHidesMethodTest4()
        {
            var expected = false;
            var actual = testAnalysis.ClassOverridesOrHidesMethod(testAnalysis.GetClassDeclSyntax("ExampleCode3.C1"), testAnalysis.GetMethodDeclSyntax("ExampleCode3.B.O"));
            Assert.AreEqual(expected, actual);
        }

        // GetInheritanceTree tests:
        // =========================
        [TestMethod]
        public void GetInheritanceTreeTest1()
        {
            var expected = new StaticCodeAnalysis.InheritanceTree(
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.M"),
                new InheritanceNode(
                    testAnalysis.GetClassDeclSyntax("ExampleCode3.A"),
                    new List<InheritanceNode>
                    {
                        new InheritanceNode(
                            testAnalysis.GetClassDeclSyntax("ExampleCode3.B"),
                            new List<InheritanceNode>
                            {
                                new InheritanceNode(testAnalysis.GetClassDeclSyntax("ExampleCode3.C1")),
                                new InheritanceNode(testAnalysis.GetClassDeclSyntax("ExampleCode3.C2"))
                            })
                    })
               );
            var actual = testAnalysis.GetInheritanceTree(testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.M"));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetInheritanceTreeTest2()
        {
            var expected = new StaticCodeAnalysis.InheritanceTree(
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.N"),
                new InheritanceNode(
                    testAnalysis.GetClassDeclSyntax("ExampleCode3.A"),
                    new List<InheritanceNode> { })
               );
            var actual = testAnalysis.GetInheritanceTree(testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.N"));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetInheritanceTreeTest3()
        {
            var expected = new StaticCodeAnalysis.InheritanceTree(
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.O"),
                new InheritanceNode(
                    testAnalysis.GetClassDeclSyntax("ExampleCode3.A"),
                    new List<InheritanceNode> { })
               );
            var actual = testAnalysis.GetInheritanceTree(testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.O"));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetInheritanceTreeTest4()
        {
            var expected = new StaticCodeAnalysis.InheritanceTree(
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.B.O"),
                new InheritanceNode(
                    testAnalysis.GetClassDeclSyntax("ExampleCode3.B"),
                    new List<InheritanceNode>
                    {
                        new InheritanceNode(testAnalysis.GetClassDeclSyntax("ExampleCode3.C1")),
                        new InheritanceNode(testAnalysis.GetClassDeclSyntax("ExampleCode3.C2"))
                    })
               );
            var actual = testAnalysis.GetInheritanceTree(testAnalysis.GetMethodDeclSyntax("ExampleCode3.B.O"));
            Assert.AreEqual(expected, actual);
        }

        // GetAllNonStaticMethods test:
        // ============================
        [TestMethod]
        public void GetAllNonStaticMethodsTest()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.M"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.N"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.O"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.B.N"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.B.O"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.C1.N"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.D.M"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.D.N")
            };
            var actual = testAnalysis.GetAllNonStaticMethods();
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetInvocations test:
        // ====================
        [TestMethod()]
        public void GetInvocationsTest()
        {
            var expected = new List<StaticCodeAnalysis.Invocation>
            {
                new StaticCodeAnalysis.Invocation(
                    new List<int> { 29, 30, 32 },
                    new List<MethodDeclarationSyntax>
                    {
                        testAnalysis.GetMethodDeclSyntax("ExampleCode3.B.N"),
                        testAnalysis.GetMethodDeclSyntax("ExampleCode3.C1.N")
                    }),
                new StaticCodeAnalysis.Invocation(
                    new List<int> { 31, 33 },
                    new List<MethodDeclarationSyntax>
                    {
                        testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.N"),
                        testAnalysis.GetMethodDeclSyntax("ExampleCode3.B.N"),
                        testAnalysis.GetMethodDeclSyntax("ExampleCode3.C1.N"),
                        testAnalysis.GetMethodDeclSyntax("ExampleCode3.D.N")
                    })
            };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.M"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetInterfaceMethodImplementingMethods test:
        // ===========================================
        [TestMethod()]
        public void GetInterfaceMethodImplementingMethodsTest()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.N"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.D.N")
            };
            var actual = testAnalysis.GetInterfaceMethodImplementingMethods(testAnalysis.GetMethodDeclSyntax("ExampleCode3.IA.N"));
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
