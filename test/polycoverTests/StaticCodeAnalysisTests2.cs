﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using polycover;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace polycover.Tests
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
            var expected = new List<ClassDeclarationSyntax> { };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest2()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetBaseClassesTest3()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"),
                testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass")
            };
            var actual = testAnalysis.GetBaseClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetDirectDerivedClasses tests:
        // ==============================
        [TestMethod]
        public void GetDirectDerivedClassesTest1()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass")
            };
            var actual = testAnalysis.GetDirectDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDirectDerivedClassesTest2()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass")
            };
            var actual = testAnalysis.GetDirectDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDirectDerivedClassesTest3()
        {
            var expected = new List<ClassDeclarationSyntax> { };
            var actual = testAnalysis.GetDirectDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetDerivedClasses tests:
        // ========================
        [TestMethod()]
        public void GetDerivedClassesTest1()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"),
                testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.BaseClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest2()
        {
            var expected = new List<ClassDeclarationSyntax>
            {
                testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass")
            };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedAbstractClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDerivedClassesTest3()
        {
            var expected = new List<ClassDeclarationSyntax> { };
            var actual = testAnalysis.GetDerivedClasses(testAnalysis.GetClassDeclSyntax("ExampleCode2.DerivedConcreteClass"));
            CollectionAssert.AreEqual(expected, actual);
        }

        // GetAllMethods test:
        // ===================
        [TestMethod()]
        public void GetAllMethodsTest()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("int ExampleCode2.BaseClass.Method1(int)"),
                testAnalysis.GetMethodDeclSyntax("int ExampleCode2.DerivedAbstractClass.Method1(int)"),
                testAnalysis.GetMethodDeclSyntax("double ExampleCode2.DerivedAbstractClass.Method2()"),
                testAnalysis.GetMethodDeclSyntax("double ExampleCode2.DerivedConcreteClass.Method2()"),
                testAnalysis.GetMethodDeclSyntax("bool ExampleCode2.DerivedConcreteClass.Method3(bool)")
            };
            var actual = testAnalysis.GetAllMethods();
            CollectionAssert.AreEqual(expected, actual);
        }
        
        // GetOverridingMethods tests:
        // ===========================
        [TestMethod()]
        public void GetOverridingMethodsTest1()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("int ExampleCode2.DerivedAbstractClass.Method1(int)")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("int ExampleCode2.BaseClass.Method1(int)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest2()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("int ExampleCode2.DerivedAbstractClass.Method1(int)"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest3()
        {
            var expected = new List<MethodDeclarationSyntax>
            {
                testAnalysis.GetMethodDeclSyntax("double ExampleCode2.DerivedConcreteClass.Method2()")
            };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("double ExampleCode2.DerivedAbstractClass.Method2()"));
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetOverridingMethodsTest4()
        {
            var expected = new List<MethodDeclarationSyntax> { };
            var actual = testAnalysis.GetOverridingMethods(testAnalysis.GetMethodDeclSyntax("double ExampleCode2.DerivedConcreteClass.Method2()"));
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
