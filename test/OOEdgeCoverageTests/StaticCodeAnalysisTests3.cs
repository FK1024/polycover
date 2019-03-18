using Microsoft.VisualStudio.TestTools.UnitTesting;
using OOEdgeCoverage;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OOEdgeCoverage.Tests
{
    [TestClass()]
    public class StaticCodeAnalysisTests3
    {
        static string path = @"..\..\..\ExampleCode\ExampleCode3.cs";
        StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(path);

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
                    testAnalysis.GetMethodDeclSyntax("ExampleCode3.C.N")
                    }),
                new StaticCodeAnalysis.Invocation(
                    new List<int> { 31, 33 },
                    new List<MethodDeclarationSyntax>
                    {
                    testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.N"),
                    testAnalysis.GetMethodDeclSyntax("ExampleCode3.B.N"),
                    testAnalysis.GetMethodDeclSyntax("ExampleCode3.C.N"),
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
