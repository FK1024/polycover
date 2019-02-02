﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using StaticCodeAnalysis;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StaticCodeAnalysis.Tests
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
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.N"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.D.N")
            };
            var actual = testAnalysis.GetInvocations(testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.M"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetInterfaceMethodImplementingMethod test:
        // ==========================================
        [TestMethod()]
        public void GetInterfaceMethodImplementingMethodTest()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.A.N"),
                testAnalysis.GetMethodDeclSyntax("ExampleCode3.D.N")
            };
            var actual = testAnalysis.GetInterfaceMethodImplementingMethod(testAnalysis.GetMethodDeclSyntax("ExampleCode3.IA.N"));
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
