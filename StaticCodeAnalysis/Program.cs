using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StaticCodeAnalysis
{
    public class Program
    {
        static void Main(string[] args)
        {
            string codePath = @"..\..\..\ExampleCode\ExampleCode3.cs";
            StaticCodeAnalysis testAnalysis = new StaticCodeAnalysis(codePath);

            YoYoGraph testGraph = new YoYoGraph();

            // create the two categories of nodes: method and invocation
            testGraph.AddCategory(new YoYoGraph.Category("method", ColorTranslator.ToHtml(Color.FromArgb(Color.LightBlue.ToArgb()))));
            testGraph.AddCategory(new YoYoGraph.Category("invocation", ColorTranslator.ToHtml(Color.FromArgb(Color.LightCoral.ToArgb()))));

            // create a node for each method
            List<MethodDeclarationSyntax> methods = testAnalysis.GetAllMethods();

            foreach (MethodDeclarationSyntax method in methods)
            {
                string nodeName = testAnalysis.GetFullMethodName(method);
                testGraph.AddNode(new YoYoGraph.Node("method", method, nodeName, nodeName));
            }

            // create a node for each invocation and add the links
            List<YoYoGraph.Node> methodNodes = new List<YoYoGraph.Node>(testGraph.Nodes);
            int invocCount = 0;
            foreach (YoYoGraph.Node methodNode in methodNodes)
            {
                List<StaticCodeAnalysis.Invocation> invocs = testAnalysis.GetInvocations(methodNode.MethDecl);
                foreach (StaticCodeAnalysis.Invocation invoc in invocs)
                {
                    YoYoGraph.Node invocNode = new YoYoGraph.Node("invocation", null, invocCount.ToString(), "invocs");
                    testGraph.AddNode(invocNode);
                    invocCount++;
                    testGraph.AddLink(new YoYoGraph.Link(methodNode.Id, invocNode.Id));

                    foreach (MethodDeclarationSyntax invocOption in invoc.Methods)
                    {
                        testGraph.AddLink(new YoYoGraph.Link(invocNode.Id, testAnalysis.GetCorrespondingNode(testGraph, invocOption).Id));
                    }
                }
            }

            string DGMLPath = codePath.Substring(0, codePath.Length - 3) + "YoYoGraph.dgml";
            testGraph.Serialize(DGMLPath);
        }
    }
}
